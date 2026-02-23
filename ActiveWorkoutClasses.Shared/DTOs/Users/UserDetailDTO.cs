using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Shared.DTOs.Users
{
    /// <summary>
    /// Detailed user information for management
    /// </summary>
    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Student-specific
        public DateTime? MembershipStartDate { get; set; }
        public DateTime? MembershipEndDate { get; set; }
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }

        // Instructor-specific
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certifications { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}
