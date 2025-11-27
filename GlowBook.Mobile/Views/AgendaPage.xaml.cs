using GlowBook.Mobile.ViewModels;

namespace GlowBook.Mobile.Views;

public partial class AgendaPage : ContentPage
{
    private readonly AgendaViewModel _vm;

    public AgendaPage(AgendaViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
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
            // voorkomt dat de app meteen sluit
            await DisplayAlert("Fout", ex.Message, "OK");
        }
    }
}
