using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Records the eligibility determination and final outcome for one student in one grading event.
    /// Created/updated by the eligibility engine and instructor result entry.
    /// </summary>
    public class GradingResult
    {
        [Key]
        public int Id { get; set; }

        public int GradingEventId { get; set; }
        public GradingEvent GradingEvent { get; set; } = null!;

        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        /// <summary>Computed by the eligibility engine — true if all rules passed.</summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Admin/instructor override. null = not overridden; true/false = manually set.
        /// The effective eligibility for display is <see cref="EffectiveEligibility"/>.
        /// </summary>
        public bool? IsOverriddenEligible { get; set; }

        /// <summary>
        /// JSON-encoded list of rule pass/fail messages produced by the engine.
        /// Example: ["✓ Attended 22 classes","✗ Only 60 days since last grading (need 90)"]
        /// </summary>
        [StringLength(2000)]
        public string? EligibilityNotes { get; set; }

        /// <summary>Null until the instructor records results after the event.</summary>
        public GradingResultStatus? Status { get; set; }

        /// <summary>The new belt/skill level awarded if the student passed.</summary>
        [StringLength(100)]
        public string? NewSkillLevel { get; set; }

        [StringLength(1000)]
        public string? InstructorNotes { get; set; }

        /// <summary>The effective eligibility, taking override into account.</summary>
        public bool EffectiveEligibility => IsOverriddenEligible ?? IsEligible;
    }
}
