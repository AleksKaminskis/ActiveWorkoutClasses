using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Base user entity for all user types in the system
    /// Azure AD B2C will handle authentication, this stores additional user data
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary key - matches Azure AD B2C object ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// User's email address (unique)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// User's role in the system
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// Whether the user account is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When the user was created in our system
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time user data was updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Full name for display purposes
        /// </summary>
        public string FullName => $"{FirstName} {LastName}";
    }
}
