namespace ActiveWorkoutClasses.Shared.DTOs.Attendance
{
    /// <summary>
    /// Request to mark attendance (instructor marks student)
    /// </summary>
    public class MarkAttendanceDto
    {
        public Guid ClassRegistrationId { get; set; }
        public Guid InstructorId { get; set; }
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }
}
