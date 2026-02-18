namespace ActiveWorkoutClasses.Shared.DTOs.Registrations
{
    /// <summary>
    /// Response after registration attempt
    /// </summary>
    public class RegistrationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ClassRegistrationDto? Registration { get; set; }
    }
}
