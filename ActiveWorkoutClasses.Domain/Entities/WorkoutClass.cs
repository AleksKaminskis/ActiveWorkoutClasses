using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    public class WorkoutClass
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _locationName = string.Empty;

        [Key]
        public Guid Id { get; set; }

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

        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters")]
        public string Description
        {
            get => _description;
            set => _description = (value ?? string.Empty).Trim();
        }

        public ClassType ClassType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationMinutes => (int)(EndDateTime - StartDateTime).TotalMinutes;

        [Required]
        [Range(1, 100, ErrorMessage = "Maximum capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Free-text fallback location name (used when no <see cref="Location"/> FK is set,
        /// or for display when the Location entity is not loaded).
        /// </summary>
        [StringLength(100)]
        public string LocationName
        {
            get => _locationName;
            set => _locationName = (value ?? string.Empty).Trim();
        }

        /// <summary>Optional FK to a managed <see cref="Location"/> entity.</summary>
        public int? LocationId { get; set; }

        /// <summary>Navigation property to the managed location (may be null for ad-hoc classes).</summary>
        public Location? Location { get; set; }

        /// <summary>
        /// If this class was generated from a recurring schedule, this links back to it.
        /// </summary>
        public int? RecurringScheduleId { get; set; }
        public RecurringSchedule? RecurringSchedule { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ClassInstructor> ClassInstructors { get; set; } = new List<ClassInstructor>();

        public ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

        public int CurrentEnrollment => ClassRegistrations.Count(r => r.Status == RegistrationStatus.Registered);

        public int AvailableSpots => MaxCapacity - CurrentEnrollment;

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
