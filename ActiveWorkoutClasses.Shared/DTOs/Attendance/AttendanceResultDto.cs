namespace ActiveWorkoutClasses.Shared.DTOs.Attendance
{
    /// <summary>
    /// Response after attendance operation
    /// </summary>
    public class AttendanceResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AttendanceDto? Attendance { get; set; }
    }
}
