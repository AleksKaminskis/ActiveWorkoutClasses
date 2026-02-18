using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Shared.DTOs.Attendance
{
    /// <summary>
    /// Attendance record information
    /// </summary>
    public class AttendanceDto
    {
        public Guid Id { get; set; }
        public Guid ClassRegistrationId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public DateTime? CheckInTime { get; set; }
        public CheckInMethod CheckInMethod { get; set; }
        public string CheckInMethodName { get; set; } = string.Empty;
        public Guid? MarkedByInstructorId { get; set; }
        public string? MarkedByInstructorName { get; set; }
        public string? Notes { get; set; }
        public bool IsPresent { get; set; }
    }
}
