using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// A belt grading, promotion, or certification event.
    /// The eligibility engine evaluates each student against the attached <see cref="EligibilityRules"/>
    /// before the event date.
    /// </summary>
    public class GradingEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public int? LocationId { get; set; }
        public Location? Location { get; set; }

        /// <summary>Which discipline this grading covers (e.g. BJJ, KravMaga).</summary>
        public ClassType ClassType { get; set; }

        /// <summary>
        /// When true the event is visible to students and the eligibility check can be run.
        /// Set by admin via the Publish action.
        /// </summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>
        /// When true individual GradingResult records are visible to students.
        /// Set by admin after recording all results.
        /// </summary>
        public bool ResultsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<EligibilityRule> EligibilityRules { get; set; } = [];
        public ICollection<GradingResult> Results { get; set; } = [];
    }
}
