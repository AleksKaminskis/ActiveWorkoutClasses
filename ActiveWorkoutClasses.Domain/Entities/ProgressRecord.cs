using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// Records a student's skill level or belt promotion in a specific discipline.
    /// One record per promotion event — the most recent record for a discipline is the student's current level.
    /// </summary>
    public class ProgressRecord
    {
        [Key]
        public int Id { get; set; }

        public Guid StudentId { get; set; }
        public Student Student { get; set; } = null!;

        public Guid? RecordedByInstructorId { get; set; }
        public Instructor? RecordedByInstructor { get; set; }

        public ClassType Discipline { get; set; }

        /// <summary>E.g. "White Belt", "Blue Belt", "Beginner", "Intermediate".</summary>
        [Required]
        [StringLength(100)]
        public string SkillLevel { get; set; } = string.Empty;

        public DateTime PromotionDate { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
