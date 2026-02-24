using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Application.DTOs.Classes
{
    /// <summary>
    /// Request to update an existing workout class
    /// </summary>
    public class UpdateWorkoutClassDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ClassType ClassType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int MaxCapacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<Guid> InstructorIds { get; set; } = new();
        public Guid? PrimaryInstructorId { get; set; }
    }
}
