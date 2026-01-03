using GlowBook.Model.Entities;
using System.Net.Http.Json;

namespace GlowBook.Mobile.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SettingsService _settings;
    private readonly AuthService _auth;

    public ApiClient(HttpClient http, SettingsService settings, AuthService auth)
    {
        _http = http;
        _settings = settings;
        _auth = auth;
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(DateTime? from = null, DateTime? to = null)
    {
        var url = "appointments";
        var q = new List<string>();
        if (from.HasValue) q.Add($"from={from.Value:s}");
        if (to.HasValue) q.Add($"to={to.Value:s}");
        if (q.Any()) url += "?" + string.Join("&", q);

        return await _http.GetFromJsonAsync<List<Appointment>>(url) ?? new();
    }

    public async Task<List<Customer>> GetCustomersAsync()
        => await _http.GetFromJsonAsync<List<Customer>>("customers") ?? new();

    public async Task<List<Service>> GetServicesAsync()
        => await _http.GetFromJsonAsync<List<Service>>("services") ?? new();

    public async Task<List<Staff>> GetStaffAsync()
    {
        return await _http.GetFromJsonAsync<List<Staff>>("staff") ?? new();
    }

    public async Task<Appointment> CreateAppointmentAsync(Appointment appt)
    {
        var resp = await _http.PostAsJsonAsync("appointments", appt);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Appointment>())!;
    }

}

