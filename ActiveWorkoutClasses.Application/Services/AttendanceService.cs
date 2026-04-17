using System.Text.Json;
using ActiveWorkoutClasses.Application.DTOs.Attendance;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    /// <summary>
    /// Service for attendance and check-in business logic
    /// </summary>
    public class AttendanceService : IAttendanceService
    {
        private readonly ApplicationDbContext _context;

        public AttendanceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CheckInResultDto> StudentCheckInAsync(Guid registrationId, Guid studentId)
        {
            // Get registration with related data
            var registration = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass)
                .Include(cr => cr.Attendance)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Registration not found",
                    Errors = new List<string> { "Invalid registration" }
                };
            }

            // Verify student owns this registration
            if (registration.StudentId != studentId)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    Errors = new List<string> { "This registration does not belong to you" }
                };
            }

            // Check if already checked in
            if (registration.Attendance != null)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Already checked in",
                    Errors = new List<string> { $"You checked in at {registration.Attendance.CheckInTime:HH:mm}" }
                };
            }

            // Verify check-in window (30 minutes before until class ends)
            var now = DateTime.UtcNow;
            if (!registration.WorkoutClass.CanCheckIn(now))
            {
                var windowStart = registration.WorkoutClass.StartDateTime.AddMinutes(-30);
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Check-in window not open",
                    Errors = new List<string>
                {
                    $"Check-in opens at {windowStart:HH:mm} and closes at {registration.WorkoutClass.EndDateTime:HH:mm}"
                }
                };
            }

            // Create attendance record
            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                ClassRegistrationId = registrationId,
                CheckInTime = now,
                CheckInMethod = CheckInMethod.SelfCheckIn,
                IsPresent = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return new CheckInResultDto
            {
                Success = true,
                Message = "Successfully checked in!",
                Attendance = new AttendanceDto
                {
                    Id = attendance.Id,
                    ClassRegistrationId = attendance.ClassRegistrationId,
                    CheckInTime = attendance.CheckInTime,
                    CheckInMethod = attendance.CheckInMethod,
                    CheckInMethodName = "Self Check-In",
                    IsPresent = attendance.IsPresent
                }
            };
        }

        public async Task<CheckInResultDto> InstructorMarkAttendanceAsync(
            Guid registrationId,
            Guid instructorId,
            bool isPresent,
            string? notes = null)
        {
            // Verify instructor exists and is active
            var instructor = await _context.Instructors.FindAsync(instructorId);
            if (instructor == null || !instructor.IsActive)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Instructor not found or inactive",
                    Errors = new List<string> { "Invalid instructor" }
                };
            }

            // Get registration
            var registration = await _context.ClassRegistrations
                .Include(cr => cr.WorkoutClass)
                .Include(cr => cr.Student)
                .Include(cr => cr.Attendance)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Registration not found",
                    Errors = new List<string> { "Invalid registration" }
                };
            }

            // Check if instructor is assigned to this class
            var isAssigned = await _context.ClassInstructors
                .AnyAsync(ci => ci.WorkoutClassId == registration.WorkoutClassId && ci.InstructorId == instructorId);

            if (!isAssigned)
            {
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Unauthorized",
                    Errors = new List<string> { "You are not assigned to this class" }
                };
            }

            var now = DateTime.UtcNow;

            // If attendance already exists, update it
            if (registration.Attendance != null)
            {
                var oldValue = JsonSerializer.Serialize(new { registration.Attendance.IsPresent, registration.Attendance.CheckInMethod });
                registration.Attendance.IsPresent = isPresent;
                registration.Attendance.MarkedByInstructorId = instructorId;
                registration.Attendance.Notes = notes;
                registration.Attendance.UpdatedAt = now;

                // If marking present and no check-in time, set it
                if (isPresent && registration.Attendance.CheckInTime == null)
                {
                    registration.Attendance.CheckInTime = now;
                }

                _context.AuditLogs.Add(new AuditLog
                {
                    EntityType = "Attendance",
                    EntityId = registration.Attendance.Id.ToString(),
                    Action = "InstructorOverride",
                    OldValue = oldValue,
                    NewValue = JsonSerializer.Serialize(new { IsPresent = isPresent, CheckInMethod = "InstructorCheckIn" }),
                    PerformedByUserId = instructorId,
                    PerformedAt = now,
                    Reason = notes
                });

                await _context.SaveChangesAsync();

                // Reload instructor info
                await _context.Entry(registration.Attendance)
                    .Reference(a => a.MarkedByInstructor)
                    .LoadAsync();

                return new CheckInResultDto
                {
                    Success = true,
                    Message = $"Attendance updated: {(isPresent ? "Present" : "Absent")}",
                    Attendance = MapAttendanceToDto(registration.Attendance)
                };
            }

            // Create new attendance record
            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                ClassRegistrationId = registrationId,
                CheckInTime = isPresent ? now : null,
                CheckInMethod = CheckInMethod.InstructorCheckIn,
                MarkedByInstructorId = instructorId,
                IsPresent = isPresent,
                Notes = notes,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // Reload instructor info
            await _context.Entry(attendance)
                .Reference(a => a.MarkedByInstructor)
                .LoadAsync();

            return new CheckInResultDto
            {
                Success = true,
                Message = $"Attendance marked: {(isPresent ? "Present" : "Absent")}",
                Attendance = MapAttendanceToDto(attendance)
            };
        }

        public async Task<AttendanceDto?> GetAttendanceByRegistrationAsync(Guid registrationId)
        {
            var attendance = await _context.Attendances
                .Include(a => a.MarkedByInstructor)
                .FirstOrDefaultAsync(a => a.ClassRegistrationId == registrationId);

            return attendance == null ? null : MapAttendanceToDto(attendance);
        }

        public async Task<List<StudentAttendanceDto>> GetClassAttendanceAsync(Guid workoutClassId)
        {
            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.Attendance)
                .Where(cr => cr.WorkoutClassId == workoutClassId && cr.Status == RegistrationStatus.Registered)
                .OrderBy(cr => cr.Student.LastName)
                .ThenBy(cr => cr.Student.FirstName)
                .ToListAsync();

            return registrations.Select(r => new StudentAttendanceDto
            {
                RegistrationId = r.Id,
                StudentId = r.StudentId,
                StudentName = r.Student.FullName,
                StudentEmail = r.Student.Email,
                RegistrationDate = r.RegistrationDate,
                HasCheckedIn = r.Attendance != null,
                CheckInTime = r.Attendance?.CheckInTime,
                IsMarkedPresent = r.Attendance?.IsPresent ?? false,
                CheckInMethod = r.Attendance?.CheckInMethod,
                Notes = r.Attendance?.Notes
            }).ToList();
        }

        public async Task<CheckInEligibilityDto> CanCheckInAsync(Guid registrationId)
        {
            var registration = await _context.ClassRegistrations
                .Include(cr => cr.WorkoutClass)
                .Include(cr => cr.Attendance)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null)
            {
                return new CheckInEligibilityDto
                {
                    CanCheckIn = false,
                    Reason = "Registration not found"
                };
            }

            if (registration.Attendance != null)
            {
                return new CheckInEligibilityDto
                {
                    CanCheckIn = false,
                    Reason = "Already checked in"
                };
            }

            var now = DateTime.UtcNow;
            var windowStart = registration.WorkoutClass.StartDateTime.AddMinutes(-30);
            var windowEnd = registration.WorkoutClass.EndDateTime;

            if (now < windowStart)
            {
                return new CheckInEligibilityDto
                {
                    CanCheckIn = false,
                    Reason = $"Check-in opens at {windowStart:HH:mm}",
                    WindowStartTime = windowStart,
                    WindowEndTime = windowEnd
                };
            }

            if (now > windowEnd)
            {
                return new CheckInEligibilityDto
                {
                    CanCheckIn = false,
                    Reason = "Check-in window has closed",
                    WindowStartTime = windowStart,
                    WindowEndTime = windowEnd
                };
            }

            return new CheckInEligibilityDto
            {
                CanCheckIn = true,
                Reason = "Ready to check in",
                WindowStartTime = windowStart,
                WindowEndTime = windowEnd
            };
        }

        public async Task<AttendanceStatsDto> GetClassAttendanceStatsAsync(Guid workoutClassId)
        {
            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassRegistrations)
                    .ThenInclude(cr => cr.Attendance)
                .FirstOrDefaultAsync(c => c.Id == workoutClassId);

            if (workoutClass == null)
            {
                throw new KeyNotFoundException($"Class with ID {workoutClassId} not found");
            }

            var registrations = workoutClass.ClassRegistrations
                .Where(cr => cr.Status == RegistrationStatus.Registered)
                .ToList();

            var totalRegistered = registrations.Count;
            var totalPresent = registrations.Count(r => r.Attendance?.IsPresent == true);
            var totalAbsent = totalRegistered - totalPresent;
            var selfCheckedIn = registrations.Count(r => r.Attendance?.CheckInMethod == CheckInMethod.SelfCheckIn);
            var instructorMarked = registrations.Count(r => r.Attendance?.CheckInMethod == CheckInMethod.InstructorCheckIn);

            return new AttendanceStatsDto
            {
                WorkoutClassId = workoutClassId,
                ClassName = workoutClass.Title,
                TotalRegistered = totalRegistered,
                TotalPresent = totalPresent,
                TotalAbsent = totalAbsent,
                SelfCheckedIn = selfCheckedIn,
                InstructorMarked = instructorMarked,
                AttendanceRate = totalRegistered > 0 ? (decimal)totalPresent / totalRegistered * 100 : 0
            };
        }

        public async Task<List<AttendanceDto>> GetStudentAttendanceHistoryAsync(Guid studentId)
        {
            var attendances = await _context.Attendances
                .Include(a => a.ClassRegistration)
                    .ThenInclude(cr => cr.WorkoutClass)
                .Include(a => a.MarkedByInstructor)
                .Where(a => a.ClassRegistration.StudentId == studentId)
                .OrderByDescending(a => a.ClassRegistration.WorkoutClass.StartDateTime)
                .ToListAsync();

            return attendances.Select(MapAttendanceToDto).ToList();
        }

        public async Task<CheckInResultDto> CheckInByStudentNumberAsync(string studentNumber, Guid workoutClassId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber && s.IsActive);

            if (student is null)
                return new CheckInResultDto { Success = false, Message = "Student not found", Errors = ["Invalid student number"] };

            var registration = await _context.ClassRegistrations
                .Include(r => r.WorkoutClass)
                .Include(r => r.Attendance)
                .FirstOrDefaultAsync(r => r.StudentId == student.Id
                    && r.WorkoutClassId == workoutClassId
                    && r.Status == RegistrationStatus.Registered);

            if (registration is null)
                return new CheckInResultDto { Success = false, Message = "No active registration found for this class", Errors = ["Not registered"] };

            if (registration.Attendance is not null)
                return new CheckInResultDto { Success = false, Message = "Already checked in", Errors = [$"Checked in at {registration.Attendance.CheckInTime:HH:mm}"] };

            var now = DateTime.UtcNow;
            if (!registration.WorkoutClass.CanCheckIn(now))
            {
                var windowStart = registration.WorkoutClass.StartDateTime.AddMinutes(-30);
                return new CheckInResultDto
                {
                    Success = false,
                    Message = "Check-in window not open",
                    Errors = [$"Opens at {windowStart:HH:mm}, closes at {registration.WorkoutClass.EndDateTime:HH:mm}"]
                };
            }

            var attendance = new Attendance
            {
                Id = Guid.NewGuid(),
                ClassRegistrationId = registration.Id,
                CheckInTime = now,
                CheckInMethod = CheckInMethod.SelfCheckIn,
                IsPresent = true,
                CreatedAt = now,
                UpdatedAt = now
            };
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return new CheckInResultDto
            {
                Success = true,
                Message = $"Welcome {student.FirstName}! Checked in to {registration.WorkoutClass.Title}.",
                Attendance = new AttendanceDto
                {
                    Id = attendance.Id,
                    ClassRegistrationId = attendance.ClassRegistrationId,
                    CheckInTime = attendance.CheckInTime,
                    CheckInMethod = attendance.CheckInMethod,
                    CheckInMethodName = "Self Check-In",
                    IsPresent = attendance.IsPresent
                }
            };
        }

        #region Private Mapping Methods

        private AttendanceDto MapAttendanceToDto(Attendance attendance)
        {
            return new AttendanceDto
            {
                Id = attendance.Id,
                ClassRegistrationId = attendance.ClassRegistrationId,
                CheckInTime = attendance.CheckInTime,
                CheckInMethod = attendance.CheckInMethod,
                CheckInMethodName = attendance.CheckInMethod.ToString(),
                IsPresent = attendance.IsPresent,
                Notes = attendance.Notes,
                MarkedByInstructorId = attendance.MarkedByInstructorId,
                MarkedByInstructorName = attendance.MarkedByInstructor?.FullName
            };
        }

        #endregion
    }
}
