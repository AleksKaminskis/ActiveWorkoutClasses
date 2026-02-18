using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class ClassRegistration
    {
        /// <summary>
        /// Unique identifier for this registration
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The student who registered
        /// </summary>
        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        /// <summary>
        /// The class they registered for
        /// </summary>
        public Guid WorkoutClassId { get; set; }
        public WorkoutClass WorkoutClass { get; set; } = null!;

        /// <summary>
        /// When the student registered for this class
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Current status of this registration
        /// </summary>
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        /// <summary>
        /// Attendance record for this registration (if class has occurred)
        /// </summary>
        public Attendance? Attendance { get; set; }

        /// <summary>
        /// Check if student has checked in or been marked present
        /// </summary>
        public bool HasAttendance => Attendance != null && Attendance.IsPresent;
    }
}
