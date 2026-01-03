using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Models.Offline;
using GlowBook.Mobile.Services;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace GlowBook.Mobile.ViewModels;

public partial class CreateAppointmentViewModel : BaseViewModel
{
    private readonly LocalDatabase _db;
    private readonly SyncService _sync;

    public ObservableCollection<LocalCustomer> Customers { get; } = new();
    public ObservableCollection<LocalStaff> StaffMembers { get; } = new();
    public ObservableCollection<LocalService> Services { get; } = new();

    [ObservableProperty] private LocalCustomer? selectedCustomer;
    [ObservableProperty] private LocalStaff? selectedStaff;
    [ObservableProperty] private LocalService? selectedService;

    [ObservableProperty] private DateTime startDate = DateTime.Today;
    [ObservableProperty] private TimeSpan startTime = new(9, 0, 0);

    [ObservableProperty] private DateTime endDate = DateTime.Today;
    [ObservableProperty] private TimeSpan endTime = new(10, 0, 0);

    [ObservableProperty] private string status = "Ingepland";
    [ObservableProperty] private string error = "";

    public CreateAppointmentViewModel(LocalDatabase db, SyncService sync)
    {
        Title = "Nieuwe afspraak";
        _db = db;
        _sync = sync;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        Error = "";

        await _sync.PullLookupsAsync();

        var customers = await _db.GetCustomersAsync();
        var staff = await _db.GetStaffAsync();
        var services = await _db.GetServicesAsync();

        Customers.Clear();
        foreach (var c in customers) Customers.Add(c);

        StaffMembers.Clear();
        foreach (var s in staff) StaffMembers.Add(s);

        Services.Clear();
        foreach (var s in services) Services.Add(s);

        if (Customers.Count == 0 || StaffMembers.Count == 0 || Services.Count == 0)
            Error = "Synchroniseer 1x online zodat klanten/medewerkers/diensten lokaal beschikbaar zijn.";
    }

    partial void OnSelectedServiceChanged(LocalService? value)
    {
        if (value == null) return;

        var start = Combine(StartDate, StartTime);
        var end = start.AddMinutes(Math.Max(15, value.DurationMinutes));

        EndDate = end.Date;
        EndTime = end.TimeOfDay;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = "";

        try
        {
            if (SelectedCustomer == null || SelectedStaff == null || SelectedService == null)
            {
                Error = "Selecteer klant, medewerker en service.";
                return;
            }

            var start = Combine(StartDate, StartTime);
            var end = Combine(EndDate, EndTime);

            if (end <= start)
            {
                Error = "Eindtijd moet na starttijd liggen.";
                return;
            }

            var draft = new AppointmentDraft
            {
                LocalUid = Guid.NewGuid().ToString("N"),
                CustomerId = SelectedCustomer.ServerId,
                StaffId = SelectedStaff.ServerId,
                Start = start,
                End = end,
                Status = Status,
                ServiceIds = new[] { SelectedService.ServerId }
            };

            var payloadJson = JsonSerializer.Serialize(draft);

            var localRow = new LocalAppointmentV2
            {
                LocalUid = draft.LocalUid,
                ServerId = 0,
                Start = draft.Start,
                End = draft.End,
                CustomerId = draft.CustomerId,
                StaffId = draft.StaffId,
                ServiceIdsCsv = string.Join(",", draft.ServiceIds),

                CustomerName = SelectedCustomer.Name,
                StaffName = SelectedStaff.Name,
                ServiceName = SelectedService.Name,
                Status = draft.Status,
                IsPending = true,
                UpdatedUtc = DateTime.UtcNow
            };

            await _db.EnqueueCreateAppointmentAsync(localRow, payloadJson);

            await _sync.TrySyncEverythingAsync();

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static DateTime Combine(DateTime date, TimeSpan time)
        => date.Date.Add(time);
}
