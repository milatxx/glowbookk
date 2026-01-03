using GlowBook.Mobile.Models.Offline;
using GlowBook.Model.Entities;
using System.Text.Json;

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

    public bool IsOnline()
        => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public async Task TrySyncEverythingAsync()
    {
        if (!IsOnline())
            return;

        await PushPendingAsync();

        await PullLookupsAsync();

        await PullAppointmentsAsync();
    }

    public async Task PullLookupsAsync()
    {
        if (!IsOnline()) return;

        try
        {
            var customers = await _api.GetCustomersAsync();
            var services = await _api.GetServicesAsync();
            var staff = await _api.GetStaffAsync();

            await _db.SaveCustomersAsync(customers);
            await _db.SaveServicesAsync(services);
            await _db.SaveStaffAsync(staff);
        }
        catch
        {
        }
    }

    public async Task PullAppointmentsAsync()
    {
        if (!IsOnline()) return;

        var from = DateTime.Today.AddDays(-7);
        var to = DateTime.Today.AddDays(7);

        try
        {
            var online = await _api.GetAppointmentsAsync(from, to);
            await _db.MergeServerAppointmentsAsync(online);
        }
        catch
        {
        }
    }

    private async Task PushPendingAsync()
    {
        if (!IsOnline()) return;

        var pending = await _db.GetPendingChangesAsync();
        if (pending.Count == 0) return;

        foreach (var change in pending)
        {
            try
            {
                if (change.Operation == "CreateAppointment")
                {
                    var draft = JsonSerializer.Deserialize<AppointmentDraft>(change.PayloadJson)
                                ?? throw new Exception("Ongeldige draft JSON");

                    var appt = new Appointment
                    {
                        CustomerId = draft.CustomerId,
                        StaffId = draft.StaffId,
                        Start = draft.Start,
                        End = draft.End,
                        Status = draft.Status,
                        AppointmentServices = draft.ServiceIds.Select(id => new AppointmentService
                        {
                            ServiceId = id
                        }).ToList()
                    };

                    var created = await _api.CreateAppointmentAsync(appt);

                    await _db.MarkLocalAppointmentSyncedAsync(draft.LocalUid, created.Id, created.Status);
                    await _db.DeletePendingChangeAsync(change.Id);
                }
                else
                {
                }
            }
            catch
            {
                return;
            }
        }
    }
}
