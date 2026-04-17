using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// A single rule that a student must satisfy to be eligible for a <see cref="GradingEvent"/>.
    /// </summary>
    public class EligibilityRule
    {
        [Key]
        public int Id { get; set; }

        public int GradingEventId { get; set; }
        public GradingEvent GradingEvent { get; set; } = null!;

        public EligibilityRuleType RuleType { get; set; }

        /// <summary>
        /// Integer threshold used by count-based rules
        /// (e.g. 20 for <see cref="EligibilityRuleType.MinAttendanceCount"/>).
        /// </summary>
        public int? IntValue { get; set; }

        /// <summary>
        /// Day-count threshold used by time-based rules
        /// (e.g. 90 for <see cref="EligibilityRuleType.MinDaysSinceLastGrading"/>).
        /// </summary>
        public int? DaysValue { get; set; }

        /// <summary>Human-readable explanation shown to students when they fail this rule.</summary>
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
