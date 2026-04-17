using ActiveWorkoutClasses.Application.DTOs.Auth;

namespace ActiveWorkoutClasses.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<Guid> RegisterStudentAsync(RegisterRequestDto dto);
        Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
        Task<UserDto?> GetCurrentUserAsync(Guid userId);
    }
}
