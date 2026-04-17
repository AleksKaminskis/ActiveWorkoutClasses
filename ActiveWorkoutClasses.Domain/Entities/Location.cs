using System.ComponentModel.DataAnnotations;

namespace ActiveWorkoutClasses.Domain.Entities
{
    /// <summary>
    /// A physical venue where training classes are held.
    /// </summary>
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Range(1, 500)]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<WorkoutClass> Classes { get; set; } = [];
        public ICollection<RecurringSchedule> Schedules { get; set; } = [];
        public ICollection<GradingEvent> GradingEvents { get; set; } = [];
    }
}
