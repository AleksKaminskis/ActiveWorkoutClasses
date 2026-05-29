using ActiveWorkoutClasses.Domain.Enums;
using ActiveWorkoutClasses.Application.DTOs.Attendance;

namespace ActiveWorkoutClasses.Application.DTOs.Registrations
{
    /// <summary>
    /// Class registration information
    /// </summary>
    public class ClassRegistrationDto
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public Guid WorkoutClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public DateTime ClassStartTime { get; set; }
        public DateTime ClassEndTime { get; set; }
        public string ClassLocation { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public RegistrationStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public bool HasAttendance { get; set; }
        public AttendanceDto? Attendance { get; set; }
    }
}
