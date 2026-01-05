using Microsoft.Maui.Storage;

namespace GlowBook.Mobile.Services;

public class SettingsService
{
    private const string ApiBaseUrlKey = "api_base_url";

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

    private static string GetDefaultApiBaseUrl()
    {
        // Android emulator -> via 10.0.2.2 naar mijn pc
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5293/api/";

        // Windows/desktop
        return "https://localhost:7129/api/";
    }
}
