using ActiveWorkoutClasses.Application.DTOs.Classes;
using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    /// <summary>
    /// Service interface for workout class operations
    /// </summary>
    public interface IWorkoutClassService
    {
        /// <summary>
        /// Get all active workout classes
        /// </summary>
        Task<List<WorkoutClassDto>> GetAllClassesAsync();

        /// <summary>
        /// Get upcoming classes (starting from now)
        /// </summary>
        Task<List<WorkoutClassDto>> GetUpcomingClassesAsync();

        /// <summary>
        /// Get classes by type
        /// </summary>
        Task<List<WorkoutClassDto>> GetClassesByTypeAsync(ClassType classType);

        /// <summary>
        /// Get a specific class by ID
        /// </summary>
        Task<WorkoutClassDto?> GetClassByIdAsync(Guid id);

        /// <summary>
        /// Get classes with pagination
        /// </summary>
        Task<WorkoutClassListDto> GetClassesPagedAsync(int page, int pageSize);

        /// <summary>
        /// Create a new workout class
        /// </summary>
        Task<WorkoutClassDto> CreateClassAsync(CreateWorkoutClassDto createDto);

        /// <summary>
        /// Update an existing workout class
        /// </summary>
        Task<WorkoutClassDto> UpdateClassAsync(Guid id, UpdateWorkoutClassDto updateDto);

        /// <summary>
        /// Delete a workout class
        /// </summary>
        Task<bool> DeleteClassAsync(Guid id);

        /// <summary>
        /// Check if a class exists
        /// </summary>
        Task<bool> ClassExistsAsync(Guid id);

        /// <summary>
        /// Get available spots for a class
        /// </summary>
        Task<int> GetAvailableSpotsAsync(Guid classId);
    }
}
