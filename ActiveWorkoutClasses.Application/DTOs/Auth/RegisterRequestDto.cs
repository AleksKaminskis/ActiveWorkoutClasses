using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Application.DTOs.Auth
{
    /// <summary>
    /// Request to register a new user account
    /// </summary>
    public class RegisterRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Student;

        // Student-specific fields (optional)
        public string EmergencyContact { get; set; } = string.Empty;
        public string MedicalNotes { get; set; } = string.Empty;

        // Instructor-specific fields (optional)
        public string Specialization { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public int? YearsOfExperience { get; set; }
        public string Certifications { get; set; } = string.Empty;
    }

}
