using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Maui.Storage;

namespace GlowBook.Mobile.Services;

public class AuthService
{
    private readonly HttpClient _http;

    private const string TokenKey = "gb_token";
    private const string ExpiresKey = "gb_expires_utc";

    public string? AccessToken { get; private set; }
    public DateTime? ExpiresUtc { get; private set; }

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        var token = await SecureStorage.Default.GetAsync(TokenKey);
        var expStr = await SecureStorage.Default.GetAsync(ExpiresKey);

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(expStr))
            return false;

        if (!DateTime.TryParse(expStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var expUtc))
            return false;

        if (DateTime.UtcNow >= expUtc)
        {
            await LogoutAsync();
            return false;
        }

        AccessToken = token;
        ExpiresUtc = expUtc;

        ApplyAuthHeader();
        return true;
    }

    public record LoginRequest(string Email, string Password);
    public record LoginResponse(string AccessToken, DateTime ExpiresUtc, string Email, string DisplayName, string[] Roles, string[] Permissions);

    public async Task LoginAsync(string email, string password)
    {
        var resp = await _http.PostAsJsonAsync("auth/login", new LoginRequest(email, password));

        if (!resp.IsSuccessStatusCode)
        {
            var msg = await resp.Content.ReadAsStringAsync();
            throw new Exception(string.IsNullOrWhiteSpace(msg) ? "Login mislukt" : msg);
        }

        var data = await resp.Content.ReadFromJsonAsync<LoginResponse>()
                   ?? throw new Exception("Ongeldig login antwoord.");

        AccessToken = data.AccessToken;
        ExpiresUtc = data.ExpiresUtc;

        await SecureStorage.Default.SetAsync(TokenKey, data.AccessToken);
        await SecureStorage.Default.SetAsync(ExpiresKey, data.ExpiresUtc.ToString("O"));

        ApplyAuthHeader();
    }

    public async Task LogoutAsync()
    {
        AccessToken = null;
        ExpiresUtc = null;

        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(ExpiresKey);

        _http.DefaultRequestHeaders.Authorization = null;
        await Task.CompletedTask;
    }

    private void ApplyAuthHeader()
    {
        if (!string.IsNullOrWhiteSpace(AccessToken))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
    }
}
