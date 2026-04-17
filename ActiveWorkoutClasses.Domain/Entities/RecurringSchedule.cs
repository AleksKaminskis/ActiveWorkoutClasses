using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// A recurring weekly class definition.
    /// When created (or materialised manually) it generates individual WorkoutClass instances
    /// for each occurrence up to <see cref="GeneratedWeeksAhead"/> weeks into the future.
    /// </summary>
    public class RecurringSchedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public ClassType ClassType { get; set; }

        /// <summary>Day of the week this class repeats on.</summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>Local start time (stored as UTC offset-agnostic time-of-day).</summary>
        public TimeOnly StartTime { get; set; }

        /// <summary>Local end time.</summary>
        public TimeOnly EndTime { get; set; }

        [Range(1, 200)]
        public int MaxCapacity { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>First day this schedule is valid — used as the anchor for occurrence generation.</summary>
        public DateOnly ValidFrom { get; set; }

        /// <summary>Optional end date. If null the schedule runs indefinitely.</summary>
        public DateOnly? ValidUntil { get; set; }

        /// <summary>How many weeks ahead to generate WorkoutClass instances when materialising.</summary>
        public int GeneratedWeeksAhead { get; set; } = 12;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation — the materialised class instances generated from this schedule
        public ICollection<WorkoutClass> GeneratedClasses { get; set; } = [];
    }
}
