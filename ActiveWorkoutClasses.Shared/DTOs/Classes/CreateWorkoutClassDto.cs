using ActiveWorkoutClasses.Domain.Enums;
using System.ComponentModel.DataAnnotations;


namespace ActiveWorkoutClasses.Application.DTOs.Classes
{
    /// <summary>
    /// Request to create a new workout class
    /// </summary>
    public class CreateWorkoutClassDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be 3-200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description must be at most 1000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ClassType ClassType { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Capacity must be between 1 and 100")]
        public int MaxCapacity { get; set; }

        public string Location { get; set; } = string.Empty;
        public int? LocationId { get; set; }
        public List<Guid> InstructorIds { get; set; } = new();
        public Guid? PrimaryInstructorId { get; set; }
    }
}
