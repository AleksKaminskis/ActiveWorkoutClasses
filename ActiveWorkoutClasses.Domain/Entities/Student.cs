
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
        public string EmergencyContact { get; set; } = string.Empty;

        /// <summary>
        /// Any medical conditions or notes the studio should be aware of
        /// </summary>
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
