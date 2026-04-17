using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Immutable audit trail for sensitive operations such as attendance overrides
    /// and eligibility overrides.  No FK constraints are used so records survive entity deletion.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        /// <summary>Name of the entity type affected, e.g. "Attendance", "GradingResult".</summary>
        [Required]
        [StringLength(100)]
        public string EntityType { get; set; } = string.Empty;

        /// <summary>String representation of the affected entity's primary key.</summary>
        [Required]
        [StringLength(100)]
        public string EntityId { get; set; } = string.Empty;

        /// <summary>Short action label, e.g. "Override", "ManualCheckIn", "EligibilityOverride".</summary>
        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        /// <summary>JSON snapshot of the entity before the change.</summary>
        public string? OldValue { get; set; }

        /// <summary>JSON snapshot of the entity after the change.</summary>
        public string? NewValue { get; set; }

        /// <summary>ID of the user who performed the action (stored as string — no FK).</summary>
        [Required]
        public Guid PerformedByUserId { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}
