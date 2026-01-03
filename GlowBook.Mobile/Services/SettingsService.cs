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

    public static string GetDefaultApiBaseUrl()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
            return "http://10.0.2.2:5293/api/"; 
        return "http://localhost:5293/api/";
    }
}
