namespace ActiveWorkoutClasses.Application.DTOs.Attendance
{
    /// <summary>
    /// Result of check-in attempt
    /// </summary>
    public class CheckInResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AttendanceDto? Attendance { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
