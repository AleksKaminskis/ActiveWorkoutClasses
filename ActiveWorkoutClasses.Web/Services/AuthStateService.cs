using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ActiveWorkoutClasses.Web.Services
{
    public class AuthStateService : IAuthStateService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;
        private readonly JwtAuthStateProvider _authStateProvider;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        public AuthStateService(
            ILocalStorageService localStorage,
            HttpClient http,
            AuthenticationStateProvider authStateProvider)
        {
            _localStorage = localStorage;
            _http = http;
            _authStateProvider = (JwtAuthStateProvider)authStateProvider;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserRole);
        public string? UserRole { get; private set; }
        public string? UserId { get; private set; }
        public string? StudentNumber { get; private set; }
        public string? FullName { get; private set; }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { Email = email, Password = password });
            if (!response.IsSuccessStatusCode) return false;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
            if (result?.AccessToken is null) return false;

            await _localStorage.SetItemAsStringAsync("jwt_token", result.AccessToken);
            _authStateProvider.NotifyUserLogin(result.AccessToken);

            UserRole = result.User?.Role;
            UserId = result.User?.Id.ToString();
            FullName = result.User?.FullName;
            StudentNumber = ExtractClaimFromJwt(result.AccessToken, "studentNumber");

            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.AccessToken);

            return true;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("jwt_token");
            _http.DefaultRequestHeaders.Authorization = null;
            UserRole = null;
            UserId = null;
            StudentNumber = null;
            FullName = null;
            _authStateProvider.NotifyUserLogout();
        }

        // Local minimal DTOs — avoids cross-namespace import from Shared
        private class LoginResponse
        {
            public string? AccessToken { get; set; }
            public LoginUser? User { get; set; }
        }
        private class LoginUser
        {
            public Guid Id { get; set; }
            public string? FullName { get; set; }
            public string? Role { get; set; }
        }

        private static string? ExtractClaimFromJwt(string jwt, string claimKey)
        {
            try
            {
                var payload = jwt.Split('.')[1];
                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                var jsonBytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
                var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes);
                return claims?.TryGetValue(claimKey, out var val) == true ? val.ToString() : null;
            }
            catch { return null; }
        }
    }
}
