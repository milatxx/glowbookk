using GlowBook.Mobile.Services;
using GlowBook.Mobile.Views;

namespace GlowBook.Mobile;

public partial class App : Application
{
    private readonly IServiceProvider _sp;
    private readonly AuthService _auth;

    public App(AuthService auth, IServiceProvider sp)
    {
        InitializeComponent();
        _sp = sp;
        _auth = auth;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loadingPage = new ContentPage
        {
            Content = new ActivityIndicator
            {
                IsRunning = true,
                VerticalOptions = LayoutOptions.Center
            }
        };

        var window = new Window(loadingPage);

        _ = InitAsync(window);

        return window;
    }

    private async Task InitAsync(Window window)
    {
        var ok = await _auth.TryRestoreSessionAsync();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            window.Page = ok
                ? new AppShell()
                : _sp.GetRequiredService<LoginPage>();
        });
    }
}
