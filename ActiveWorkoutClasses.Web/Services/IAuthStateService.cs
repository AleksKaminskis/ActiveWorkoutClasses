namespace ActiveWorkoutClasses.Web.Services
{
    public interface IAuthStateService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        bool IsAuthenticated { get; }
        string? UserRole { get; }
        string? UserId { get; }
        string? StudentNumber { get; }
        string? FullName { get; }
    }
}
