using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Base user entity for all user types in the system
    /// Azure AD B2C will handle authentication, this stores additional user data
    /// </summary>
    public class User
    {
        private string _email = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _phoneNumber = string.Empty;

        /// <summary>
        /// Primary key - matches Azure AD B2C object ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// User's email address (unique)
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Email cannot be empty");

                if (value.Length > 256)
                    throw new ArgumentException("Email cannot exceed 256 characters");

                if (!IsValidEmail(value))
                    throw new ArgumentException("Invalid email format");

                _email = value.Trim().ToLowerInvariant();
            }
        }

        /// <summary>
        /// First name
        /// </summary>
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("First name cannot be empty");

                if (value.Length < 2 || value.Length > 100)
                    throw new ArgumentException("First name must be between 2 and 100 characters");

                if (!IsValidName(value))
                    throw new ArgumentException("First name contains invalid characters. Only letters, spaces, hyphens, and apostrophes are allowed");

                _firstName = value.Trim();
            }
        }

        /// <summary>
        /// Last name
        /// </summary>
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
        public string LastName
        {
            get => _lastName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Last name cannot be empty");

                if (value.Length < 2 || value.Length > 100)
                    throw new ArgumentException("Last name must be between 2 and 100 characters");

                if (!IsValidName(value))
                    throw new ArgumentException("Last name contains invalid characters. Only letters, spaces, hyphens, and apostrophes are allowed");

                _lastName = value.Trim();
            }
        }

        /// <summary>
        /// Contact phone number
        /// </summary>
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Phone number cannot be empty");

                // Remove common formatting characters for validation
                var digitsOnly = new string(value.Where(char.IsDigit).ToArray());

                if (digitsOnly.Length < 10)
                    throw new ArgumentException("Phone number must contain at least 10 digits");

                if (digitsOnly.Length > 15)
                    throw new ArgumentException("Phone number cannot exceed 15 digits");

                _phoneNumber = value.Trim();
            }
        }

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

        /// <summary>
        /// Validates email format using regex
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Basic email validation regex
                var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates name (allows letters, spaces, hyphens, apostrophes)
        /// </summary>
        private static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow letters (any language), spaces, hyphens, apostrophes
            var nameRegex = new Regex(@"^[\p{L}\s\-']+$", RegexOptions.IgnoreCase);
            return nameRegex.IsMatch(name);
        }

        /// <summary>
        /// Validates the entire user object
        /// </summary>
        public virtual void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);

            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                throw new ValidationException($"User validation failed: {errors}");
            }
        }
    }
}
