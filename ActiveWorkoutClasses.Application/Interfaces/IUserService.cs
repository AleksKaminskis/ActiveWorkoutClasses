using ActiveWorkoutClasses.Application.DTOs.Users;
using ActiveWorkoutClasses.Domain.Enums;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    /// <summary>
    /// Service interface for user management operations
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get all users
        /// </summary>
        Task<List<UserDto>> GetAllUsersAsync();

        /// <summary>
        /// Get users by role
        /// </summary>
        Task<List<UserDto>> GetUsersByRoleAsync(UserRole role);

        /// <summary>
        /// Get all students
        /// </summary>
        Task<List<UserDetailDto>> GetAllStudentsAsync();

        /// <summary>
        /// Get all instructors
        /// </summary>
        Task<List<UserDetailDto>> GetAllInstructorsAsync();

        /// <summary>
        /// Get user by ID with full details
        /// </summary>
        Task<UserDetailDto?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Get user by email
        /// </summary>
        Task<UserDto?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Get users with pagination
        /// </summary>
        Task<UserListDto> GetUsersPagedAsync(int page, int pageSize, UserRole? role = null);

        /// <summary>
        /// Create a new user
        /// </summary>
        Task<UserDetailDto> CreateUserAsync(CreateUserDto createDto);

        /// <summary>
        /// Update existing user
        /// </summary>
        Task<UserDetailDto> UpdateUserAsync(Guid id, UpdateUserDto updateDto);

        /// <summary>
        /// Deactivate a user (soft delete)
        /// </summary>
        Task<bool> DeactivateUserAsync(Guid id);

        /// <summary>
        /// Activate a user
        /// </summary>
        Task<bool> ActivateUserAsync(Guid id);

        /// <summary>
        /// Delete a user permanently (hard delete)
        /// </summary>
        Task<bool> DeleteUserAsync(Guid id);

        /// <summary>
        /// Check if user exists
        /// </summary>
        Task<bool> UserExistsAsync(Guid id);

        /// <summary>
        /// Check if email is already in use
        /// </summary>
        Task<bool> EmailExistsAsync(string email);
    }
}
