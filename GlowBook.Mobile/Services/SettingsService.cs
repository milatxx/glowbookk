using Microsoft.Maui.Storage;

namespace GlowBook.Mobile.Services;

public class SettingsService
{
    private const string ApiBaseUrlKey = "api_base_url";
    private const string LastSyncKey = "last_sync_utc";

    public string ApiBaseUrl
    {
        get => Preferences.Default.Get(ApiBaseUrlKey, GetDefaultApiBaseUrl());
        set
        {
            var v = (value ?? "").Trim();
            if (!v.EndsWith("/")) v += "/";
            Preferences.Default.Set(ApiBaseUrlKey, v);
        }
    }

    // Tijdstip van laatste geslaagde synchronisatie (lokaal opgeslagen)
    public DateTime? LastSyncUtc
    {
        get
        {
            var ticks = Preferences.Default.Get(LastSyncKey, 0L);
            return ticks == 0L ? null : new DateTime(ticks, DateTimeKind.Utc);
        }
        set => Preferences.Default.Set(LastSyncKey, value?.ToUniversalTime().Ticks ?? 0L);
    }

    private static string GetDefaultApiBaseUrl()
    {
        // Android emulator -> via 10.0.2.2 naar mijn pc
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5293/api/";

        // Windows/desktop
        return "https://localhost:7129/api/";
    }
}
