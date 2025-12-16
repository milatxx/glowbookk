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

        // HttpClient voor API-calls
        builder.Services.AddSingleton<HttpClient>();

        // Services
        builder.Services.AddSingleton<ApiClient>();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "glowbook_mobile.db3");
        builder.Services.AddSingleton(_ => new LocalDatabase(dbPath));
        builder.Services.AddSingleton<SyncService>();

        // ViewModels
        builder.Services.AddTransient<AgendaViewModel>();
        builder.Services.AddTransient<CustomersViewModel>();
        builder.Services.AddTransient<ServicesViewModel>();

        // Views
        builder.Services.AddTransient<AgendaPage>();
        builder.Services.AddTransient<CustomersPage>();
        builder.Services.AddTransient<ServicesPage>();

        return builder.Build();
    }
}
