using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;

namespace ActiveWorkoutClasses.Web.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly ILocalStorageService _localStorage;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public ApiClient(HttpClient http, ILocalStorageService localStorage)
        {
            _http = http;
            _localStorage = localStorage;
        }

        public async Task EnsureTokenAttachedAsync()
        {
            if (_http.DefaultRequestHeaders.Authorization is null)
            {
                var token = await _localStorage.GetItemAsStringAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            }
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            await EnsureTokenAttachedAsync();
            return await _http.GetFromJsonAsync<T>(url, _jsonOptions);
        }

        public async Task<T?> PostAsync<T>(string url, object body)
        {
            await EnsureTokenAttachedAsync();
            var response = await _http.PostAsJsonAsync(url, body, _jsonOptions);
            response.EnsureSuccessStatusCode();
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default;
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<T?> PutAsync<T>(string url, object body)
        {
            await EnsureTokenAttachedAsync();
            var response = await _http.PutAsJsonAsync(url, body, _jsonOptions);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
        }

        public async Task<bool> DeleteAsync(string url)
        {
            await EnsureTokenAttachedAsync();
            var response = await _http.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
    }
}
