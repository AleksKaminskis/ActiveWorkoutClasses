using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Application.DTOs.Classes
{
    /// <summary>
    /// Workout class data for display
    /// </summary>
    public class WorkoutClassDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ClassType ClassType { get; set; }
        public string ClassTypeName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxCapacity { get; set; }
        public int CurrentEnrollment { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsFull { get; set; }
        public string Location { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public bool IsActive { get; set; }
        public List<InstructorSummaryDto> Instructors { get; set; } = new();
        public bool CanCheckIn { get; set; }
        public bool IsToday { get; set; }
    }
}
