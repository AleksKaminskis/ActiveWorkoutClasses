using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class WorkoutClass
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _location = string.Empty;

        /// <summary>
        /// Unique identifier for the class
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Class title (e.g., "Morning Krav Maga Fundamentals")
        /// </summary>
        [Required(ErrorMessage = "Class title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Title cannot be empty");

                if (value.Length < 3 || value.Length > 200)
                    throw new ArgumentException("Title must be between 3 and 200 characters");

                _title = value.Trim();
            }
        }

        /// <summary>
        /// Detailed description of what the class covers
        /// </summary>
        [Required(ErrorMessage = "Class description is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 1000 characters")]
        public string Description
        {
            get => _description;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Description cannot be empty");

                if (value.Length < 10 || value.Length > 1000)
                    throw new ArgumentException("Description must be between 10 and 1000 characters");

                _description = value.Trim();
            }
        }

        /// <summary>
        /// Type of workout class
        /// </summary>
        public ClassType ClassType { get; set; }

        /// <summary>
        /// When the class starts
        /// </summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// When the class ends
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Class duration in minutes (calculated from start/end)
        /// </summary>
        public int DurationMinutes => (int)(EndDateTime - StartDateTime).TotalMinutes;

        /// <summary>
        /// Maximum number of students allowed
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Maximum capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Physical location (e.g., "Studio A", "Main Hall")
        /// </summary>
        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Location must be between 2 and 100 characters")]
        public string Location
        {
            get => _location;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Location cannot be empty");

                if (value.Length < 2 || value.Length > 100)
                    throw new ArgumentException("Location must be between 2 and 100 characters");

                _location = value.Trim();
            }
        }

        /// <summary>
        /// Whether this class is active/published
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// When this class was created in the system
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last time class details were updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instructors teaching this class (many-to-many relationship)
        /// </summary>
        public ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

        /// <summary>
        /// Students registered for this class
        /// </summary>
        public ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

        /// <summary>
        /// Current number of registered students
        /// </summary>
        public int CurrentEnrollment => ClassRegistrations.Count(r => r.Status == RegistrationStatus.Registered);

        /// <summary>
        /// Available spots remaining
        /// </summary>
        public int AvailableSpots => MaxCapacity - CurrentEnrollment;

        /// <summary>
        /// Whether the class is full
        /// </summary>
        public bool IsFull => CurrentEnrollment >= MaxCapacity;

        /// <summary>
        /// Check if students can check-in for this class
        /// Rule: 30 minutes before class starts until class ends
        /// </summary>
        public bool CanCheckIn(DateTime currentTime)
        {
            var checkInWindowStart = StartDateTime.AddMinutes(-30);
            return currentTime >= checkInWindowStart && currentTime <= EndDateTime;
        }

        /// <summary>
        /// Check if this class is happening today
        /// </summary>
        public bool IsToday(DateTime currentDate)
        {
            return StartDateTime.Date == currentDate.Date;
        }

        /// Validates the workout class
        /// </summary>
        public void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(this);

            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(vr => vr.ErrorMessage));
                throw new ValidationException($"Workout class validation failed: {errors}");
            }

            // Custom business rule validations
            if (EndDateTime <= StartDateTime)
                throw new ValidationException("End time must be after start time");

            if (DurationMinutes < 15)
                throw new ValidationException("Class must be at least 15 minutes long");

            if (DurationMinutes > 240)
                throw new ValidationException("Class cannot exceed 4 hours");

            if (StartDateTime < DateTime.UtcNow.AddHours(-1))
                throw new ValidationException("Cannot create class in the past");

            if (MaxCapacity < 1 || MaxCapacity > 100)
                throw new ValidationException("Maximum capacity must be between 1 and 100");
        }
    }
}
