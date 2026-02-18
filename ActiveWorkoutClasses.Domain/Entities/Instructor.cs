namespace ActiveWorkoutClasses.Domain.Entities
{
    public class Instructor : User
    {
        /// <summary>
        /// Primary area of expertise (e.g., "Krav Maga & Self-Defense")
        /// </summary>
        public string Specialization { get; set; } = string.Empty;

        /// <summary>
        /// Instructor biography for student-facing profiles
        /// </summary>
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// Years of teaching/training experience
        /// </summary>
        public int YearsOfExperience { get; set; }

        /// <summary>
        /// Professional certifications (comma-separated or JSON)
        /// </summary>
        public string Certifications { get; set; } = string.Empty;

        /// <summary>
        /// Hourly rate for payroll purposes (optional)
        /// </summary>
        public decimal? HourlyRate { get; set; }

        /// <summary>
        /// Classes this instructor teaches
        /// Navigation property through ClassInstructor (many-to-many)
        /// </summary>
        public ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();
    }
}
