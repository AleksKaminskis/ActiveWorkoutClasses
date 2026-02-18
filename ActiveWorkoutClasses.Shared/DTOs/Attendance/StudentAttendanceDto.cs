using ActiveWorkoutClasses.Domain.Enums;


namespace ActiveWorkoutClasses.Shared.DTOs.Attendance
{
    /// <summary>
    /// Student attendance information for instructor view
    /// </summary>
    public class StudentAttendanceDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public Guid RegistrationId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool HasCheckedIn { get; set; }
        public DateTime? CheckInTime { get; set; }
        public bool IsMarkedPresent { get; set; }
        public CheckInMethod? CheckInMethod { get; set; }
        public string? Notes { get; set; }
    }
}