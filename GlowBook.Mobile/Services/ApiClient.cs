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

    // Afspraken
    public async Task<List<Appointment>> GetAppointmentsAsync(DateTime? from = null, DateTime? to = null)
    {
        var url = "appointments";
        var q = new List<string>();

        if (from.HasValue) q.Add($"from={from.Value:s}");
        if (to.HasValue) q.Add($"to={to.Value:s}");

        if (q.Any()) url += "?" + string.Join("&", q);

        return await _http.GetFromJsonAsync<List<Appointment>>(url) ?? new();
    }

    public async Task<Appointment?> GetAppointmentAsync(int id)
        => await _http.GetFromJsonAsync<Appointment>($"appointments/{id}");

    public async Task<Appointment> CreateAppointmentAsync(Appointment appt)
    {
        var resp = await _http.PostAsJsonAsync("appointments", appt);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Appointment>())!;
    }

    public async Task UpdateAppointmentAsync(int id, Appointment appt)
    {
        var resp = await _http.PutAsJsonAsync($"appointments/{id}", appt);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteAppointmentAsync(int id)
    {
        var resp = await _http.DeleteAsync($"appointments/{id}");
        resp.EnsureSuccessStatusCode();
    }

    // Klanten
    public async Task<List<Customer>> GetCustomersAsync()
        => await _http.GetFromJsonAsync<List<Customer>>("customers") ?? new();

    public async Task<Customer?> GetCustomerAsync(int id)
        => await _http.GetFromJsonAsync<Customer>($"customers/{id}");

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        var resp = await _http.PostAsJsonAsync("customers", customer);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Customer>())!;
    }

    public async Task UpdateCustomerAsync(int id, Customer customer)
    {
        var resp = await _http.PutAsJsonAsync($"customers/{id}", customer);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var resp = await _http.DeleteAsync($"customers/{id}");
        resp.EnsureSuccessStatusCode();
    }

    // Diensten
    public async Task<List<Service>> GetServicesAsync()
        => await _http.GetFromJsonAsync<List<Service>>("services") ?? new();

    public async Task<Service?> GetServiceAsync(int id)
        => await _http.GetFromJsonAsync<Service>($"services/{id}");

    public async Task<Service> CreateServiceAsync(Service service)
    {
        var resp = await _http.PostAsJsonAsync("services", service);
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<Service>())!;
    }

    public async Task UpdateServiceAsync(int id, Service service)
    {
        var resp = await _http.PutAsJsonAsync($"services/{id}", service);
        resp.EnsureSuccessStatusCode();
    }

    public async Task DeleteServiceAsync(int id)
    {
        var resp = await _http.DeleteAsync($"services/{id}");
        resp.EnsureSuccessStatusCode();
    }

    // Medewerkers
    public async Task<List<Staff>> GetStaffAsync()
        => await _http.GetFromJsonAsync<List<Staff>>("staff") ?? new();

    public async Task<Staff?> GetStaffMemberAsync(int id)
        => await _http.GetFromJsonAsync<Staff>($"staff/{id}");
}
