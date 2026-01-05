using GlowBook.Mobile.Models.Offline;
using GlowBook.Model.Entities;
using SQLite;

namespace GlowBook.Mobile.Services;

public class LocalDatabase
{
    private readonly SQLiteAsyncConnection _db;
    private bool _initialized;

    public LocalDatabase(string path)
    {
        _db = new SQLiteAsyncConnection(path);
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        await _db.CreateTableAsync<LocalCustomer>();
        await _db.CreateTableAsync<LocalService>();
        await _db.CreateTableAsync<LocalStaff>();

        await _db.CreateTableAsync<LocalAppointmentV2>();
        await _db.CreateTableAsync<PendingChange>();

        _initialized = true;
    }

    public async Task SaveCustomersAsync(IEnumerable<Customer> customers)
    {
        await EnsureInitializedAsync();

        var items = customers.Select(c => new LocalCustomer
        {
            ServerId = c.Id,
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email
        }).ToList();

        await _db.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<LocalCustomer>();
            tran.InsertAll(items);
        });
    }

    public async Task SaveServicesAsync(IEnumerable<Service> services)
    {
        await EnsureInitializedAsync();

        var items = services.Select(s => new LocalService
        {
            ServerId = s.Id,
            Name = s.Name,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            Category = s.Category
        }).ToList();

        await _db.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<LocalService>();
            tran.InsertAll(items);
        });
    }

    public async Task SaveStaffAsync(IEnumerable<Staff> staff)
    {
        await EnsureInitializedAsync();

        var items = staff.Select(s => new LocalStaff
        {
            ServerId = s.Id,
            Name = s.Name,
            RoleName = s.RoleName,
            Email = s.Email
        }).ToList();

        await _db.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<LocalStaff>();
            tran.InsertAll(items);
        });
    }

    public async Task<List<LocalCustomer>> GetCustomersAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalCustomer>().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<List<LocalService>> GetServicesAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalService>().OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<List<LocalStaff>> GetStaffAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<LocalStaff>().OrderBy(s => s.Name).ToListAsync();
    }
    
    public async Task<List<LocalAppointmentV2>> GetAppointmentsAsync()
    {
        await EnsureInitializedAsync();

        return await _db.Table<LocalAppointmentV2>()
            .OrderBy(a => a.Start)
            .ToListAsync();
    }

    public async Task MergeServerAppointmentsAsync(IEnumerable<Appointment> items)
    {
        await EnsureInitializedAsync();

        var mapped = items.Select(a =>
        {
            var firstService = a.AppointmentServices?.FirstOrDefault()?.Service;

            return new LocalAppointmentV2
            {
                LocalUid = "",

                ServerId = a.Id,
                Start = a.Start,
                End = a.End,
                CustomerId = a.CustomerId,
                StaffId = a.StaffId,

                ServiceIdsCsv = string.Join(",", a.AppointmentServices?.Select(x => x.ServiceId) ?? Enumerable.Empty<int>()),

                CustomerName = a.Customer?.Name ?? "",
                StaffName = a.Staff?.Name ?? "",
                ServiceName = firstService?.Name ?? "",
                Status = a.Status ?? "",

                IsPending = false,
                UpdatedUtc = DateTime.UtcNow
            };
        }).ToList();

        foreach (var m in mapped)
        {
            var existing = await _db.Table<LocalAppointmentV2>()
                .Where(x => x.ServerId == m.ServerId)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Start = m.Start;
                existing.End = m.End;
                existing.CustomerId = m.CustomerId;
                existing.StaffId = m.StaffId;
                existing.ServiceIdsCsv = m.ServiceIdsCsv;
                existing.CustomerName = m.CustomerName;
                existing.StaffName = m.StaffName;
                existing.ServiceName = m.ServiceName;
                existing.Status = m.Status;
                existing.IsPending = false;
                existing.UpdatedUtc = DateTime.UtcNow;

                await _db.UpdateAsync(existing);
            }
            else
            {
                m.LocalUid = $"srv_{m.ServerId}";
                await _db.InsertAsync(m);
            }
        }
    }

    public async Task EnqueueCreateAppointmentAsync(LocalAppointmentV2 localDisplayRow, string payloadJson)
    {
        await EnsureInitializedAsync();

        // localDisplayrow komt binnen me IsPending=true en ServerId=0
        await _db.InsertAsync(localDisplayRow);

        await _db.InsertAsync(new PendingChange
        {
            Operation = "CreateAppointment",
            PayloadJson = payloadJson,
            CreatedUtc = DateTime.UtcNow
        });
    }
    public async Task EnqueueUpdateAppointmentAsync(string payloadJson)
    {
        await EnsureInitializedAsync();
        await _db.InsertAsync(new PendingChange
        {
            Operation = "UpdateAppointment",
            PayloadJson = payloadJson,
            CreatedUtc = DateTime.UtcNow
        });
    }

    public async Task EnqueueDeleteAppointmentAsync(string payloadJson)
    {
        await EnsureInitializedAsync();
        await _db.InsertAsync(new PendingChange
        {
            Operation = "DeleteAppointment",
            PayloadJson = payloadJson,
            CreatedUtc = DateTime.UtcNow
        });
    }


    public async Task<List<PendingChange>> GetPendingChangesAsync()
    {
        await EnsureInitializedAsync();
        return await _db.Table<PendingChange>()
            .OrderBy(x => x.CreatedUtc)
            .ToListAsync();
    }

    public async Task DeletePendingChangeAsync(int id)
    {
        await EnsureInitializedAsync();
        await _db.DeleteAsync<PendingChange>(id);
    }

    public async Task AddPendingChangeAsync(PendingChange change)
    {
        await EnsureInitializedAsync();
        await _db.InsertAsync(change);
    }

    public async Task RemovePendingChangeAsync(PendingChange change)
    {
        await EnsureInitializedAsync();
        await _db.DeleteAsync(change);
    }

    public async Task MarkLocalAppointmentSyncedAsync(string localUid, int serverId, string status)
    {
        await EnsureInitializedAsync();

        var row = await _db.Table<LocalAppointmentV2>()
            .Where(a => a.LocalUid == localUid)
            .FirstOrDefaultAsync();

        if (row == null) return;

        row.ServerId = serverId;
        row.IsPending = false;
        row.Status = status;
        row.UpdatedUtc = DateTime.UtcNow;

        await _db.UpdateAsync(row);
    }
}
