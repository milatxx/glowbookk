using GlowBook.Model.Entities;

namespace GlowBook.Mobile.Services;

public class SyncService
{
    private readonly ApiClient _api;
    private readonly LocalDatabase _db;

    public SyncService(ApiClient api, LocalDatabase db)
    {
        _api = api;
        _db = db;
    }

    public async Task SyncAppointmentsAsync()
    {
        var from = DateTime.Today.AddDays(-7);
        var to = DateTime.Today.AddDays(7);

        List<Appointment> online;
        try
        {
            online = await _api.GetAppointmentsAsync(from, to);
        }
        catch
        {
            // offline? doe niets
            return;
        }

        await _db.SaveAppointmentsAsync(online);
    }
}
