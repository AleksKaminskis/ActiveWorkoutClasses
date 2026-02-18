namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Join table for many-to-many relationship between WorkoutClass and Instructor
    /// Allows multiple instructors per class
    /// </summary>
    public class ClassInstructor
    {
        /// <summary>
        /// The workout class
        /// </summary>
        public Guid WorkoutClassId { get; set; }
        public WorkoutClass WorkoutClass { get; set; } = null!;

        /// <summary>
        /// The instructor teaching this class
        /// </summary>
        public Guid InstructorId { get; set; }
        public Instructor Instructor { get; set; } = null!;

        /// <summary>
        /// Whether this is the primary/lead instructor
        /// </summary>
        public bool IsPrimaryInstructor { get; set; } = false;

        /// <summary>
        /// When this instructor was assigned to the class
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
