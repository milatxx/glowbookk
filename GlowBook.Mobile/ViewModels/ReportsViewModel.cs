using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Services;
using System.Collections.ObjectModel;

namespace GlowBook.Mobile.ViewModels;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly SyncService _sync;
    private readonly LocalDatabase _db;

    [ObservableProperty] private int totalAppointments;
    [ObservableProperty] private string error = "";

    // Aantal afspraken per status 
    public ObservableCollection<StatRow> StatusStats { get; } = new();

    // Top 5 meest geboekte diensten
    public ObservableCollection<StatRow> TopServices { get; } = new();

    public ReportsViewModel(SyncService sync, LocalDatabase db)
    {
        _sync = sync;
        _db = db;
        Title = "Rapporten";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = "";

        try
        {
            // Eerst proberen te synchroniseren derna lokaal (offline) berekenen
            await _sync.TrySyncEverythingAsync();

            var list = await _db.GetAppointmentsAsync();

            TotalAppointments = list.Count;

            StatusStats.Clear();
            foreach (var g in list
                         .GroupBy(a => string.IsNullOrWhiteSpace(a.Status) ? "Onbekend" : a.Status)
                         .OrderByDescending(g => g.Count()))
            {
                StatusStats.Add(new StatRow(g.Key, g.Count()));
            }

            TopServices.Clear();
            foreach (var g in list
                         .Where(a => !string.IsNullOrWhiteSpace(a.ServiceName))
                         .GroupBy(a => a.ServiceName)
                         .OrderByDescending(g => g.Count())
                         .Take(5))
            {
                TopServices.Add(new StatRow(g.Key, g.Count()));
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}

// Eenvoudige rij vr statistiek (label + aantal)
public record StatRow(string Label, int Count);
