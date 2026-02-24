using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Application.DTOs.Classes
{
    /// <summary>
    /// Request to create a new workout class
    /// </summary>
    public class CreateWorkoutClassDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ClassType ClassType { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int MaxCapacity { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<Guid> InstructorIds { get; set; } = new();
        public Guid? PrimaryInstructorId { get; set; }
    }
}
