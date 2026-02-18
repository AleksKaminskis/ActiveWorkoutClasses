using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class Attendance
    {
        /// <summary>
        /// Unique identifier for this attendance record
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The class registration this attendance is for
        /// </summary>
        public Guid ClassRegistrationId { get; set; }
        public ClassRegistration ClassRegistration { get; set; } = null!;

        /// <summary>
        /// When the student checked in (null if marked by instructor without self check-in)
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// How the attendance was recorded
        /// </summary>
        public CheckInMethod CheckInMethod { get; set; }

        /// <summary>
        /// If marked by instructor, which instructor marked it
        /// </summary>
        public Guid? MarkedByInstructorId { get; set; }
        public Instructor? MarkedByInstructor { get; set; }

        /// <summary>
        /// Optional notes from instructor about attendance
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Whether the student was actually present
        /// True = present, False = absent (but registered)
        /// </summary>
        public bool IsPresent { get; set; }

        /// <summary>
        /// When this attendance record was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time attendance was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
