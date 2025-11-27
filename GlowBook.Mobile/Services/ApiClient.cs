using GlowBook.Model.Entities;
using System.Net.Http.Json;

namespace GlowBook.Mobile.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://localhost:7129/api/");
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
}
