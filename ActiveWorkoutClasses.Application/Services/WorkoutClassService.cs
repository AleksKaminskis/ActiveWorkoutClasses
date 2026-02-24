using ActiveWorkoutClasses.Application.DTOs.Classes;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    /// <summary>
    /// Service for workout class business logic
    /// </summary>
    public class WorkoutClassService : IWorkoutClassService
    {
        private readonly ApplicationDbContext _context;

        public WorkoutClassService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkoutClassDto>> GetAllClassesAsync()
        {
            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                .Where(c => c.IsActive)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return classes.Select(MapToDto).ToList();
        }

        public async Task<List<WorkoutClassDto>> GetUpcomingClassesAsync()
        {
            var now = DateTime.UtcNow;
            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                .Where(c => c.IsActive && c.StartDateTime > now)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return classes.Select(MapToDto).ToList();
        }

        public async Task<List<WorkoutClassDto>> GetClassesByTypeAsync(ClassType classType)
        {
            var classes = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                .Where(c => c.IsActive && c.ClassType == classType)
                .OrderBy(c => c.StartDateTime)
                .ToListAsync();

            return classes.Select(MapToDto).ToList();
        }

        public async Task<WorkoutClassDto?> GetClassByIdAsync(Guid id)
        {
            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                .FirstOrDefaultAsync(c => c.Id == id);

            return workoutClass == null ? null : MapToDto(workoutClass);
        }

        public async Task<WorkoutClassListDto> GetClassesPagedAsync(int page, int pageSize)
        {
            var query = _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                    .ThenInclude(ci => ci.Instructor)
                .Include(c => c.ClassRegistrations)
                .Where(c => c.IsActive)
                .OrderBy(c => c.StartDateTime);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var classes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new WorkoutClassListDto
            {
                Classes = classes.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<WorkoutClassDto> CreateClassAsync(CreateWorkoutClassDto createDto)
        {
            // Create the workout class entity
            var workoutClass = new WorkoutClass
            {
                Id = Guid.NewGuid(),
                Title = createDto.Title,
                Description = createDto.Description,
                ClassType = createDto.ClassType,
                StartDateTime = createDto.StartDateTime,
                EndDateTime = createDto.EndDateTime,
                MaxCapacity = createDto.MaxCapacity,
                Location = createDto.Location,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Validate the class
            workoutClass.Validate();

            // Add to context
            _context.WorkoutClasses.Add(workoutClass);

            // Assign instructors if provided
            if (createDto.InstructorIds.Any())
            {
                foreach (var instructorId in createDto.InstructorIds)
                {
                    var instructor = await _context.Instructors.FindAsync(instructorId);
                    if (instructor != null)
                    {
                        var classInstructor = new ClassInstructor
                        {
                            WorkoutClassId = workoutClass.Id,
                            InstructorId = instructorId,
                            IsPrimaryInstructor = instructorId == createDto.PrimaryInstructorId,
                            AssignedAt = DateTime.UtcNow
                        };
                        _context.ClassInstructors.Add(classInstructor);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Reload with instructors
            await _context.Entry(workoutClass)
                .Collection(c => c.ClassInstructors)
                .Query()
                .Include(ci => ci.Instructor)
                .LoadAsync();

            return MapToDto(workoutClass);
        }

        public async Task<WorkoutClassDto> UpdateClassAsync(Guid id, UpdateWorkoutClassDto updateDto)
        {
            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassInstructors)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (workoutClass == null)
            {
                throw new KeyNotFoundException($"Workout class with ID {id} not found");
            }

            // Update properties
            workoutClass.Title = updateDto.Title;
            workoutClass.Description = updateDto.Description;
            workoutClass.ClassType = updateDto.ClassType;
            workoutClass.StartDateTime = updateDto.StartDateTime;
            workoutClass.EndDateTime = updateDto.EndDateTime;
            workoutClass.MaxCapacity = updateDto.MaxCapacity;
            workoutClass.Location = updateDto.Location;
            workoutClass.IsActive = updateDto.IsActive;
            workoutClass.UpdatedAt = DateTime.UtcNow;

            // Validate updated class
            workoutClass.Validate();

            // Update instructors if provided
            if (updateDto.InstructorIds.Any())
            {
                // Remove existing instructor assignments
                _context.ClassInstructors.RemoveRange(workoutClass.ClassInstructors);

                // Add new assignments
                foreach (var instructorId in updateDto.InstructorIds)
                {
                    var instructor = await _context.Instructors.FindAsync(instructorId);
                    if (instructor != null)
                    {
                        var classInstructor = new ClassInstructor
                        {
                            WorkoutClassId = workoutClass.Id,
                            InstructorId = instructorId,
                            IsPrimaryInstructor = instructorId == updateDto.PrimaryInstructorId,
                            AssignedAt = DateTime.UtcNow
                        };
                        _context.ClassInstructors.Add(classInstructor);
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Reload with updated instructors
            await _context.Entry(workoutClass)
                .Collection(c => c.ClassInstructors)
                .Query()
                .Include(ci => ci.Instructor)
                .LoadAsync();

            await _context.Entry(workoutClass)
                .Collection(c => c.ClassRegistrations)
                .LoadAsync();

            return MapToDto(workoutClass);
        }

        public async Task<bool> DeleteClassAsync(Guid id)
        {
            var workoutClass = await _context.WorkoutClasses.FindAsync(id);
            if (workoutClass == null)
            {
                return false;
            }

            _context.WorkoutClasses.Remove(workoutClass);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ClassExistsAsync(Guid id)
        {
            return await _context.WorkoutClasses.AnyAsync(c => c.Id == id);
        }

        public async Task<int> GetAvailableSpotsAsync(Guid classId)
        {
            var workoutClass = await _context.WorkoutClasses
                .Include(c => c.ClassRegistrations)
                .FirstOrDefaultAsync(c => c.Id == classId);

            if (workoutClass == null)
            {
                throw new KeyNotFoundException($"Workout class with ID {classId} not found");
            }

            return workoutClass.AvailableSpots;
        }

        #region Private Mapping Methods

        /// <summary>
        /// Maps WorkoutClass entity to WorkoutClassDto
        /// </summary>
        private WorkoutClassDto MapToDto(WorkoutClass workoutClass)
        {
            var now = DateTime.UtcNow;

            return new WorkoutClassDto
            {
                Id = workoutClass.Id,
                Title = workoutClass.Title,
                Description = workoutClass.Description,
                ClassType = workoutClass.ClassType,
                ClassTypeName = workoutClass.ClassType.ToString(),
                StartDateTime = workoutClass.StartDateTime,
                EndDateTime = workoutClass.EndDateTime,
                DurationMinutes = workoutClass.DurationMinutes,
                MaxCapacity = workoutClass.MaxCapacity,
                CurrentEnrollment = workoutClass.CurrentEnrollment,
                AvailableSpots = workoutClass.AvailableSpots,
                IsFull = workoutClass.IsFull,
                Location = workoutClass.Location,
                IsActive = workoutClass.IsActive,
                CanCheckIn = workoutClass.CanCheckIn(now),
                IsToday = workoutClass.IsToday(now.Date),
                Instructors = workoutClass.ClassInstructors
                    .Select(ci => new InstructorSummaryDto
                    {
                        Id = ci.InstructorId,
                        FullName = ci.Instructor.FullName,
                        Specialization = ci.Instructor.Specialization,
                        IsPrimary = ci.IsPrimaryInstructor
                    })
                    .ToList()
            };
        }

        #endregion
    }
}
