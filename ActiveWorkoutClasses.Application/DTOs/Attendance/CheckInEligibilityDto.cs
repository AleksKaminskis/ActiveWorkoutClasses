namespace ActiveWorkoutClasses.Application.DTOs.Attendance
{
    /// <summary>
    /// Check if student can check-in
    /// </summary>
    public class CheckInEligibilityDto
    {
        public bool CanCheckIn { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime? WindowStartTime { get; set; }
        public DateTime? WindowEndTime { get; set; }
    }

}
