using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Services;

namespace GlowBook.Mobile.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settings;
    private readonly SyncService _sync;

    [ObservableProperty] private string apiBaseUrl = "";
    [ObservableProperty] private string lastSyncText = "";
    [ObservableProperty] private string statusMessage = "";
    [ObservableProperty] private bool isOnline;
    [ObservableProperty] private string onlineText = "";

    public SettingsViewModel(SettingsService settings, SyncService sync)
    {
        _settings = settings;
        _sync = sync;
        Title = "Instellingen";
    }

    // Laadt huidige instellingen en syncstatus in beeld
    [RelayCommand]
    public Task LoadAsync()
    {
        ApiBaseUrl = _settings.ApiBaseUrl;
        IsOnline = _sync.IsOnline();
        OnlineText = IsOnline ? "Online" : "Offline";
        RefreshLastSyncText();
        return Task.CompletedTask;
    }

    // Bewaart API-url lokaal
    [RelayCommand]
    private Task SaveAsync()
    {
        _settings.ApiBaseUrl = ApiBaseUrl;
        ApiBaseUrl = _settings.ApiBaseUrl; // genormaliseerd terug tonen 
        StatusMessage = "Instellingen opgeslagen.";
        return Task.CompletedTask;
    }

    // Handmatige synchronisatie + bijwerken van laatste sync tijd
    [RelayCommand]
    private async Task SyncNowAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "";

        try
        {
            IsOnline = _sync.IsOnline();
            OnlineText = IsOnline ? "Online" : "Offline";
            if (!IsOnline)
            {
                StatusMessage = "Geen internetverbinding. Synchronisatie gebeurt later automatisch.";
                return;
            }

            await _sync.TrySyncEverythingAsync();
            _settings.LastSyncUtc = DateTime.UtcNow;
            RefreshLastSyncText();
            StatusMessage = "Synchronisatie voltooid.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Fout bij synchroniseren: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void RefreshLastSyncText()
    {
        var last = _settings.LastSyncUtc;
        LastSyncText = last.HasValue
            ? last.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
            : "Nog niet gesynchroniseerd";
    }
}
