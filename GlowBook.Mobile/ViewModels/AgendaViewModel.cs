using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Models.Offline;
using GlowBook.Mobile.Services;
using System.Collections.ObjectModel;

namespace GlowBook.Mobile.ViewModels;

public partial class AgendaViewModel : BaseViewModel
{
    private readonly SyncService _sync;
    private readonly LocalDatabase _db;

    public ObservableCollection<LocalAppointmentV2> Appointments { get; } = new();

    public AgendaViewModel(SyncService sync, LocalDatabase db)
    {
        Title = "Agenda";
        _sync = sync;
        _db = db;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            await _sync.TrySyncEverythingAsync();

            var list = await _db.GetAppointmentsAsync();

            Appointments.Clear();
            foreach (var appt in list)
                Appointments.Add(appt);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task NewAppointmentAsync()
    {
        await Shell.Current.GoToAsync("appointment/new");
    }
}
