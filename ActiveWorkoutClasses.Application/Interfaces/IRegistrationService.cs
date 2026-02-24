using ActiveWorkoutClasses.Application.DTOs.Registrations;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    /// <summary>
    /// Service interface for student class registration operations
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        /// Register a student for a workout class
        /// </summary>
        Task<RegistrationResultDto> RegisterForClassAsync(Guid studentId, Guid workoutClassId);

        /// <summary>
        /// Get all registrations for a specific student
        /// </summary>
        Task<List<ClassRegistrationDto>> GetStudentRegistrationsAsync(Guid studentId);

        /// <summary>
        /// Get all registrations for a specific class
        /// </summary>
        Task<List<ClassRegistrationDto>> GetClassRegistrationsAsync(Guid workoutClassId);

        /// <summary>
        /// Get a specific registration by ID
        /// </summary>
        Task<ClassRegistrationDto?> GetRegistrationByIdAsync(Guid registrationId);

        /// <summary>
        /// Cancel a registration (change status to Cancelled)
        /// </summary>
        Task<bool> CancelRegistrationAsync(Guid registrationId);

        /// <summary>
        /// Check if a student is already registered for a class
        /// </summary>
        Task<bool> IsStudentRegisteredAsync(Guid studentId, Guid workoutClassId);

        /// <summary>
        /// Get upcoming registrations for a student
        /// </summary>
        Task<List<ClassRegistrationDto>> GetUpcomingRegistrationsAsync(Guid studentId);

        /// <summary>
        /// Get registrations for today's classes for a student
        /// </summary>
        Task<List<ClassRegistrationDto>> GetTodayRegistrationsAsync(Guid studentId);
    }
}
