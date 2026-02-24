namespace ActiveWorkoutClasses.Application.DTOs.Registrations
{
    /// <summary>
    /// Request to register a student for a class
    /// </summary>
    public class RegisterForClassDto
    {
        public Guid StudentId { get; set; }
        public Guid WorkoutClassId { get; set; }
    }
}
