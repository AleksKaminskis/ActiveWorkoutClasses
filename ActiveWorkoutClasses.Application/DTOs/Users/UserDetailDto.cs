using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.DTOs.Users
{
    /// <summary>
    /// Detailed user information for management
    /// </summary>
    public class UserDetailDto : UserDto
    {
        // Student-specific
        public DateTime? MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }
        public bool? HasActiveMembership { get; set; }

        // Instructor-specific
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certifications { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}
