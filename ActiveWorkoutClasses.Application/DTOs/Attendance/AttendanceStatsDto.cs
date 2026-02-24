namespace ActiveWorkoutClasses.Application.DTOs.Attendance
{
    /// <summary>
    /// Attendance statistics for a class
    /// </summary>
    public class AttendanceStatsDto
    {
        public Guid WorkoutClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int TotalRegistered { get; set; }
        public int TotalPresent { get; set; }
        public int TotalAbsent { get; set; }
        public int SelfCheckedIn { get; set; }
        public int InstructorMarked { get; set; }
        public decimal AttendanceRate { get; set; }
    }

}
