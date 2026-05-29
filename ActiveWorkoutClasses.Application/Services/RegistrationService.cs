using ActiveWorkoutClasses.Application.DTOs.Attendance;
using ActiveWorkoutClasses.Application.DTOs.Registrations;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    /// <summary>
    /// Service for student class registration business logic
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegistrationResultDto> RegisterForClassAsync(Guid studentId, Guid workoutClassId)
        {
            // Validate student exists
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Student not found",
                    Errors = new List<string> { $"Student with ID {studentId} does not exist" }
                };
            }

            // Validate student is active
            if (!student.IsActive)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Student account is not active",
                    Errors = new List<string> { "Your account has been deactivated. Please contact admin." }
                };
            }

            // Validate class exists
            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassRegistrations)
                .FirstOrDefaultAsync(c => c.Id == workoutClassId);

            if (workoutClass == null)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Class not found",
                    Errors = new List<string> { $"Class with ID {workoutClassId} does not exist" }
                };
            }

            // Validate class is active
            if (!workoutClass.IsActive)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Class is not available",
                    Errors = new List<string> { "This class is currently not accepting registrations" }
                };
            }

            // Check if class is in the past
            if (workoutClass.StartDateTime < DateTime.UtcNow)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Cannot register for past classes",
                    Errors = new List<string> { "This class has already started or ended" }
                };
            }

            // Check if already registered
            var existingRegistration = await _context.ClassRegistrations
                .FirstOrDefaultAsync(cr =>
                    cr.StudentId == studentId &&
                    cr.WorkoutClassId == workoutClassId &&
                    cr.Status == RegistrationStatus.Registered);

            if (existingRegistration != null)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Already registered",
                    Errors = new List<string> { "You are already registered for this class" }
                };
            }

            // Check if class is full
            if (workoutClass.IsFull)
            {
                return new RegistrationResultDto
                {
                    Success = false,
                    Message = "Class is full",
                    Errors = new List<string> { $"This class has reached maximum capacity ({workoutClass.MaxCapacity} students)" }
                };
            }

            // Create registration
            var registration = new ClassRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                WorkoutClassId = workoutClassId,
                RegistrationDate = DateTime.UtcNow,
                Status = RegistrationStatus.Registered
            };

            _context.ClassRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            await _context.Entry(registration)
                .Reference(r => r.Student)
                .LoadAsync();

            await _context.Entry(registration)
                .Reference(r => r.WorkoutClass)
                .LoadAsync();

            return new RegistrationResultDto
            {
                Success = true,
                Message = "Successfully registered for class",
                Registration = MapToDto(registration)
            };
        }

        public async Task<List<ClassRegistrationDto>> GetStudentRegistrationsAsync(Guid studentId)
        {
            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass).ThenInclude(wc => wc.Location)
                .Include(cr => cr.Attendance)
                .Where(cr => cr.StudentId == studentId)
                .OrderByDescending(cr => cr.WorkoutClass.StartDateTime)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        public async Task<List<ClassRegistrationDto>> GetClassRegistrationsAsync(Guid workoutClassId)
        {
            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass)
                .Include(cr => cr.Attendance)
                .Where(cr => cr.WorkoutClassId == workoutClassId && cr.Status == RegistrationStatus.Registered)
                .OrderBy(cr => cr.Student.LastName)
                .ThenBy(cr => cr.Student.FirstName)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        public async Task<ClassRegistrationDto?> GetRegistrationByIdAsync(Guid registrationId)
        {
            var registration = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass)
                .Include(cr => cr.Attendance)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            return registration == null ? null : MapToDto(registration);
        }

        public async Task<bool> CancelRegistrationAsync(Guid registrationId)
        {
            var registration = await _context.ClassRegistrations
                .Include(cr => cr.WorkoutClass)
                .FirstOrDefaultAsync(cr => cr.Id == registrationId);

            if (registration == null)
            {
                return false;
            }

            // Check if class has already started
            if (registration.WorkoutClass.StartDateTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Cannot cancel registration for a class that has already started");
            }

            // Update status to cancelled
            registration.Status = RegistrationStatus.Cancelled;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsStudentRegisteredAsync(Guid studentId, Guid workoutClassId)
        {
            return await _context.ClassRegistrations
                .AnyAsync(cr =>
                    cr.StudentId == studentId &&
                    cr.WorkoutClassId == workoutClassId &&
                    cr.Status == RegistrationStatus.Registered);
        }

        public async Task<List<ClassRegistrationDto>> GetUpcomingRegistrationsAsync(Guid studentId)
        {
            var now = DateTime.UtcNow;
            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass).ThenInclude(wc => wc.Location)
                .Include(cr => cr.Attendance)
                .Where(cr =>
                    cr.StudentId == studentId &&
                    cr.Status == RegistrationStatus.Registered &&
                    cr.WorkoutClass.StartDateTime > now)
                .OrderBy(cr => cr.WorkoutClass.StartDateTime)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        public async Task<List<ClassRegistrationDto>> GetTodayRegistrationsAsync(Guid studentId)
        {
            var today = DateTime.UtcNow.Date;
            var registrations = await _context.ClassRegistrations
                .Include(cr => cr.Student)
                .Include(cr => cr.WorkoutClass).ThenInclude(wc => wc.Location)
                .Include(cr => cr.Attendance)
                .Where(cr =>
                    cr.StudentId == studentId &&
                    cr.Status == RegistrationStatus.Registered &&
                    cr.WorkoutClass.StartDateTime.Date == today)
                .OrderBy(cr => cr.WorkoutClass.StartDateTime)
                .ToListAsync();

            return registrations.Select(MapToDto).ToList();
        }

        #region Private Mapping Methods

        private ClassRegistrationDto MapToDto(ClassRegistration registration)
        {
            return new ClassRegistrationDto
            {
                Id = registration.Id,
                StudentId = registration.StudentId,
                StudentName = registration.Student.FullName,
                StudentEmail = registration.Student.Email,
                WorkoutClassId = registration.WorkoutClassId,
                ClassName = registration.WorkoutClass.Title,
                ClassStartTime = registration.WorkoutClass.StartDateTime,
                ClassEndTime = registration.WorkoutClass.EndDateTime,
                ClassLocation = registration.WorkoutClass.Location?.Name ?? registration.WorkoutClass.LocationName,
                RegistrationDate = registration.RegistrationDate,
                Status = registration.Status,
                StatusName = registration.Status.ToString(),
                HasAttendance = registration.Attendance != null && registration.Attendance.IsPresent,
                Attendance = registration.Attendance != null ? new AttendanceDto
                {
                    Id = registration.Attendance.Id,
                    CheckInTime = registration.Attendance.CheckInTime,
                    CheckInMethod = registration.Attendance.CheckInMethod,
                    CheckInMethodName = registration.Attendance.CheckInMethod.ToString(),
                    IsPresent = registration.Attendance.IsPresent,
                    Notes = registration.Attendance.Notes
                } : null
            };
        }

        #endregion
    }
}
