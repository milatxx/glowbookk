using CommunityToolkit.Mvvm.ComponentModel;

namespace GlowBook.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    bool isBusy;

    [ObservableProperty]
    string title = string.Empty;
}
