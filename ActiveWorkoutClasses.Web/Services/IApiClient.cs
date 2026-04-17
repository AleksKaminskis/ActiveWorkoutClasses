namespace ActiveWorkoutClasses.Web.Services
{
    // Minimal typed API client interface — pages can inject this instead of raw HttpClient
    public interface IApiClient
    {
        Task<T?> GetAsync<T>(string url);
        Task<T?> PostAsync<T>(string url, object body);
        Task<T?> PutAsync<T>(string url, object body);
        Task<bool> DeleteAsync(string url);
        Task EnsureTokenAttachedAsync();
    }
}
