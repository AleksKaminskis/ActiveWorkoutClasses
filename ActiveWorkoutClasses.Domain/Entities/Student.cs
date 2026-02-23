using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class Student : User
    {
        /// <summary>
        /// When the student's membership started
        /// </summary>
        public DateTime MembershipStartDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the membership ends (null = ongoing)
        /// </summary>
        public DateTime? MembershipEndDate { get; set; }

        /// <summary>
        /// Emergency contact information
        /// </summary>
        [Required(ErrorMessage = "Emergency contact is required for students")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Emergency contact must be between 5 and 200 characters")]
        public string EmergencyContact { get; set; } = string.Empty;

        /// <summary>
        /// Any medical conditions or notes the studio should be aware of
        /// </summary>
        [StringLength(1000, ErrorMessage = "Medical notes cannot exceed 1000 characters")]
        public string? MedicalNotes { get; set; }

        /// <summary>
        /// All classes this student has registered for
        /// </summary>
        public ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

        /// <summary>
        /// Check if membership is currently active
        /// </summary>
        public bool HasActiveMembership()
        {
            if (!MembershipEndDate.HasValue)
                return true;

            return DateTime.UtcNow <= MembershipEndDate.Value;
        }
    }
}
