using GlowBook.Mobile.Services; 
using GlowBook.Mobile.ViewModels;
using GlowBook.Mobile.Views;

namespace GlowBook.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<SettingsService>();
            return new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl)
            };
        });

        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ApiClient>();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "glowbook_mobile.db3");
        builder.Services.AddSingleton(_ => new LocalDatabase(dbPath));
        builder.Services.AddSingleton<SyncService>();

        // ViewModels
        builder.Services.AddTransient<AgendaViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<ServicesViewModel>();
        builder.Services.AddTransient<CreateAppointmentViewModel>();

        // Views
        builder.Services.AddTransient<AgendaPage>();
        builder.Services.AddTransient<CustomersPage>();
        builder.Services.AddTransient<ServicesPage>();
        builder.Services.AddTransient<CreateAppointmentPage>();

        // Login
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<LoginPage>();

        return builder.Build();
    }
}
