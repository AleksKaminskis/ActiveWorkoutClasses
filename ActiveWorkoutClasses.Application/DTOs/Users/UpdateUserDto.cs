namespace ActiveWorkoutClasses.Application.DTOs.Users
{
    /// <summary>
    /// Request to update existing user
    /// </summary>
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        // Student-specific
        public string? EmergencyContact { get; set; }
        public string? MedicalNotes { get; set; }
        public DateTime? MembershipEndDate { get; set; }

        // Instructor-specific
        public string? Specialization { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Certifications { get; set; }
        public decimal? HourlyRate { get; set; }
    }
}
