using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Services;

namespace GlowBook.Mobile.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth;

    [ObservableProperty] private string email = "admin@glowbook.local";
    [ObservableProperty] private string password = "Admin123!";
    [ObservableProperty] private string error = "";

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;
        Title = "Login";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = "";

        try
        {
            await _auth.LoginAsync(Email, Password);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = new AppShell();
            });
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
