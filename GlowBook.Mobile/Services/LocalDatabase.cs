using GlowBook.Mobile.Models;
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

        await _db.CreateTableAsync<LocalAppointment>();

        _initialized = true;
    }

    public async Task<List<LocalAppointment>> GetAppointmentsAsync()
    {
        await EnsureInitializedAsync();

        return await _db.Table<LocalAppointment>()
                        .OrderBy(a => a.Start)
                        .ToListAsync();
    }

    public async Task SaveAppointmentsAsync(IEnumerable<Appointment> items)
    {
        await EnsureInitializedAsync();

        var mapped = items.Select(a => new LocalAppointment
        {
            Id = a.Id,
            Start = a.Start,
            End = a.End,
            CustomerName = a.Customer?.Name ?? string.Empty,
            StaffName = a.Staff?.Name ?? string.Empty,
            Status = a.Status ?? string.Empty
        }).ToList();

        await _db.RunInTransactionAsync(tran =>
        {
            tran.DeleteAll<LocalAppointment>();
            tran.InsertAll(mapped);
        });
    }
}
