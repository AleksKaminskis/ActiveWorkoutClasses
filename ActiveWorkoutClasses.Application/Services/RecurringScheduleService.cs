using ActiveWorkoutClasses.Application.DTOs.Locations;
using ActiveWorkoutClasses.Application.DTOs.Schedules;
using ActiveWorkoutClasses.Application.Interfaces;
using ActiveWorkoutClasses.Domain.Entities;
using ActiveWorkoutClasses.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ActiveWorkoutClasses.Application.Services
{
    public class RecurringScheduleService : IRecurringScheduleService
    {
        private readonly ApplicationDbContext _context;

        public RecurringScheduleService(ApplicationDbContext context) => _context = context;

        public async Task<List<RecurringScheduleDto>> GetAllAsync()
        {
            var schedules = await _context.RecurringSchedules
                .Include(s => s.Location)
                .Include(s => s.GeneratedClasses)
                .OrderBy(s => s.Title)
                .ToListAsync();
            return schedules.Select(MapToDto).ToList();
        }

        public async Task<RecurringScheduleDto?> GetByIdAsync(int id)
        {
            var schedule = await _context.RecurringSchedules
                .Include(s => s.Location)
                .Include(s => s.GeneratedClasses)
                .FirstOrDefaultAsync(s => s.Id == id);
            return schedule is null ? null : MapToDto(schedule);
        }

        public async Task<RecurringScheduleDto> CreateAsync(CreateRecurringScheduleDto dto)
        {
            var schedule = new RecurringSchedule
            {
                Title = dto.Title,
                Description = dto.Description,
                ClassType = dto.ClassType,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxCapacity = dto.MaxCapacity,
                LocationId = dto.LocationId,
                IsActive = true,
                ValidFrom = dto.ValidFrom,
                ValidUntil = dto.ValidUntil,
                GeneratedWeeksAhead = dto.GeneratedWeeksAhead,
                CreatedAt = DateTime.UtcNow
            };
            _context.RecurringSchedules.Add(schedule);
            await _context.SaveChangesAsync();
            await MaterialiseAsync(schedule.Id, dto.GeneratedWeeksAhead);
            return await GetByIdAsync(schedule.Id) ?? MapToDto(schedule);
        }

        public async Task<RecurringScheduleDto?> UpdateAsync(int id, UpdateRecurringScheduleDto dto)
        {
            var schedule = await _context.RecurringSchedules.FindAsync(id);
            if (schedule is null) return null;

            schedule.Title = dto.Title;
            schedule.Description = dto.Description;
            schedule.DayOfWeek = dto.DayOfWeek;
            schedule.StartTime = dto.StartTime;
            schedule.EndTime = dto.EndTime;
            schedule.MaxCapacity = dto.MaxCapacity;
            schedule.LocationId = dto.LocationId;
            schedule.IsActive = dto.IsActive;
            schedule.ValidUntil = dto.ValidUntil;
            schedule.GeneratedWeeksAhead = dto.GeneratedWeeksAhead;
            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var schedule = await _context.RecurringSchedules.FindAsync(id);
            if (schedule is null) return false;
            schedule.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MaterialiseAsync(int scheduleId, int? weeksAhead = null)
        {
            var schedule = await _context.RecurringSchedules
                .Include(s => s.GeneratedClasses)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule is null || !schedule.IsActive) return 0;

            var weeks = weeksAhead ?? schedule.GeneratedWeeksAhead;
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var startFrom = schedule.ValidFrom > today ? schedule.ValidFrom : today;
            int created = 0;

            for (int w = 0; w < weeks; w++)
            {
                var targetDate = GetNextOccurrence(startFrom.AddDays(w * 7), schedule.DayOfWeek);

                if (schedule.ValidUntil.HasValue && targetDate > schedule.ValidUntil.Value)
                    break;

                var alreadyExists = schedule.GeneratedClasses.Any(c =>
                    DateOnly.FromDateTime(c.StartDateTime) == targetDate);

                if (alreadyExists) continue;

                var startDateTime = targetDate.ToDateTime(schedule.StartTime);
                var endDateTime = targetDate.ToDateTime(schedule.EndTime);

                var workoutClass = new WorkoutClass
                {
                    Title = schedule.Title,
                    Description = schedule.Description ?? $"Recurring {schedule.ClassType} class",
                    ClassType = schedule.ClassType,
                    StartDateTime = startDateTime,
                    EndDateTime = endDateTime,
                    MaxCapacity = schedule.MaxCapacity,
                    LocationId = schedule.LocationId,
                    RecurringScheduleId = scheduleId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.WorkoutClasses.Add(workoutClass);
                created++;
            }

            await _context.SaveChangesAsync();
            return created;
        }

        private static DateOnly GetNextOccurrence(DateOnly from, DayOfWeek targetDay)
        {
            int daysUntil = ((int)targetDay - (int)from.DayOfWeek + 7) % 7;
            return from.AddDays(daysUntil);
        }

        private static RecurringScheduleDto MapToDto(RecurringSchedule s) => new()
        {
            Id = s.Id,
            Title = s.Title,
            Description = s.Description,
            ClassType = s.ClassType,
            ClassTypeName = s.ClassType.ToString(),
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            MaxCapacity = s.MaxCapacity,
            Location = s.Location is null ? null : new LocationDto
            {
                Id = s.Location.Id,
                Name = s.Location.Name,
                Address = s.Location.Address,
                City = s.Location.City,
                Capacity = s.Location.Capacity,
                IsActive = s.Location.IsActive
            },
            IsActive = s.IsActive,
            ValidFrom = s.ValidFrom,
            ValidUntil = s.ValidUntil,
            GeneratedWeeksAhead = s.GeneratedWeeksAhead,
            GeneratedClassCount = s.GeneratedClasses?.Count ?? 0
        };
    }
}
