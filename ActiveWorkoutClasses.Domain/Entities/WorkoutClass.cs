using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class WorkoutClass
    {
        /// <summary>
        /// Unique identifier for the class
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Class title (e.g., "Morning Krav Maga Fundamentals")
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of what the class covers
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Type of workout class
        /// </summary>
        public ClassType ClassType { get; set; }

        /// <summary>
        /// When the class starts
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// When the class ends
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Class duration in minutes (calculated from start/end)
        /// </summary>
        public int DurationMinutes => (int)(EndDateTime - StartDateTime).TotalMinutes;

        /// <summary>
        /// Maximum number of students allowed
        /// </summary>
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Physical location (e.g., "Studio A", "Main Hall")
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Whether this class is active/published
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When this class was created in the system
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time class details were updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instructors teaching this class (many-to-many relationship)
        /// </summary>
        public ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

        /// <summary>
        /// Students registered for this class
        /// </summary>
        public ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

        /// <summary>
        /// Current number of registered students
        /// </summary>
        public int CurrentEnrollment => ClassRegistrations.Count(r => r.Status == RegistrationStatus.Registered);

        /// <summary>
        /// Available spots remaining
        /// </summary>
        public int AvailableSpots => MaxCapacity - CurrentEnrollment;

        /// <summary>
        /// Whether the class is full
        /// </summary>
        public bool IsFull => CurrentEnrollment >= MaxCapacity;

        /// <summary>
        /// Check if students can check-in for this class
        /// Rule: 30 minutes before class starts until class ends
        /// </summary>
        public bool CanCheckIn(DateTime currentTime)
        {
            var checkInWindowStart = StartDateTime.AddMinutes(-30);
            return currentTime >= checkInWindowStart && currentTime <= EndDateTime;
        }

        /// <summary>
        /// Check if this class is happening today
        /// </summary>
        public bool IsToday(DateTime currentDate)
        {
            return StartDateTime.Date == currentDate.Date;
        }
    }
}
