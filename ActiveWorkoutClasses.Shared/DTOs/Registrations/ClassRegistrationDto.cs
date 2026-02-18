
using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Shared.DTOs.Attendance;

namespace ActiveWorkoutClasses.Shared.DTOs.Registrations
{
    /// <summary>
    /// Class registration information
    /// </summary>
    public class ClassRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid WorkoutClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime ClassStartTime { get; set; }
        public DateTime RegistrationDate { get; set; }
        public RegistrationStatus Status { get; set; }
        public bool HasAttendance { get; set; }
        public AttendanceDto? Attendance { get; set; }
    }
}
