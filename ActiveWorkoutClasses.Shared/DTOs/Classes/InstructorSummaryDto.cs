namespace ActiveWorkoutClasses.Shared.DTOs.Classes
{
    /// <summary>
    /// Brief instructor information for class displays
    /// </summary>
    public class InstructorSummaryDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
