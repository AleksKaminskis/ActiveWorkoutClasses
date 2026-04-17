using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ActiveWorkoutClasses.Web.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private static readonly AuthenticationState Anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        public JwtAuthStateProvider(ILocalStorageService localStorage)
            => _localStorage = localStorage;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync("jwt_token");
            if (string.IsNullOrWhiteSpace(token))
                return Anonymous;

            try
            {
                var claims = ParseClaimsFromJwt(token);
                var identity = new ClaimsIdentity(claims, "jwt");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return Anonymous;
            }
        }

        public void NotifyUserLogin(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
            var jsonBytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

            return keyValuePairs.Select(kvp =>
            {
                var value = kvp.Value.ValueKind == JsonValueKind.Array
                    ? string.Join(",", kvp.Value.EnumerateArray().Select(e => e.ToString()))
                    : kvp.Value.ToString();

                var claimType = kvp.Key switch
                {
                    "role" => ClaimTypes.Role,
                    "name" => ClaimTypes.Name,
                    _ => kvp.Key
                };
                return new Claim(claimType, value);
            });
        }
    }
}
