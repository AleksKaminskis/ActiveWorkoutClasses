namespace ActiveWorkoutClasses.Application.DTOs.Attendance
{
    /// <summary>
    /// Request to check-in to a class (student self check-in)
    /// </summary>
    public class CheckInRequestDto
    {
        public Guid ClassRegistrationId { get; set; }
        public Guid StudentId { get; set; }
    }
}
