using ActiveWorkoutClasses.Application.DTOs.Attendance;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    /// <summary>
    /// Service interface for attendance and check-in operations
    /// </summary>
    public interface IAttendanceService
    {
        /// <summary>
        /// Student self check-in for a class
        /// Rule: Can only check-in 30 minutes before class starts until class ends
        /// </summary>
        Task<CheckInResultDto> StudentCheckInAsync(Guid registrationId, Guid studentId);

        /// <summary>
        /// Instructor marks student attendance
        /// </summary>
        Task<CheckInResultDto> InstructorMarkAttendanceAsync(Guid registrationId, Guid instructorId, bool isPresent, string? notes = null);

        /// <summary>
        /// Get attendance for a specific registration
        /// </summary>
        Task<AttendanceDto?> GetAttendanceByRegistrationAsync(Guid registrationId);

        /// <summary>
        /// Get all attendance records for a class
        /// </summary>
        Task<List<StudentAttendanceDto>> GetClassAttendanceAsync(Guid workoutClassId);

        /// <summary>
        /// Check if a student can check-in for a class right now
        /// </summary>
        Task<CheckInEligibilityDto> CanCheckInAsync(Guid registrationId);

        /// <summary>
        /// Get attendance statistics for a class
        /// </summary>
        Task<AttendanceStatsDto> GetClassAttendanceStatsAsync(Guid workoutClassId);

        /// <summary>
        /// Get student's attendance history
        /// </summary>
        Task<List<AttendanceDto>> GetStudentAttendanceHistoryAsync(Guid studentId);
    }
}
