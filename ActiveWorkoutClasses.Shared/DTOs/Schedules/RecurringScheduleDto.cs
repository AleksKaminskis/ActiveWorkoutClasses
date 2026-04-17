using ActiveWorkoutClasses.Application.DTOs.Locations;
using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.DTOs.Schedules
{
    public class RecurringScheduleDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ClassType ClassType { get; set; }
        public string ClassTypeName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public LocationDto? Location { get; set; }
        public bool IsActive { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public int GeneratedWeeksAhead { get; set; }
        public int GeneratedClassCount { get; set; }
    }

    public class CreateRecurringScheduleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ClassType ClassType { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int? LocationId { get; set; }
        public DateOnly ValidFrom { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public int GeneratedWeeksAhead { get; set; } = 12;
    }

    public class UpdateRecurringScheduleDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public int? LocationId { get; set; }
        public bool IsActive { get; set; }
        public DateOnly? ValidUntil { get; set; }
        public int GeneratedWeeksAhead { get; set; } = 12;
    }
}
