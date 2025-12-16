using GlowBook.Mobile.ViewModels;

namespace GlowBook.Mobile.Views;

public partial class ServicesPage : ContentPage
{
    private readonly ServicesViewModel _vm;

    public ServicesPage(ServicesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _vm.LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
