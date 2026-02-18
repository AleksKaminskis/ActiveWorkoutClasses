namespace ActiveWorkoutClasses.Shared.DTOs.Auth
{
    /// <summary>
    /// Response after successful login/registration
    /// </summary>
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public UserDto User { get; set; } = null!;
    }
}
