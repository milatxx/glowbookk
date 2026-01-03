using GlowBook.Mobile.ViewModels;

namespace GlowBook.Mobile.Views;

public partial class CreateAppointmentPage : ContentPage
{
    private readonly CreateAppointmentViewModel _vm;

    public CreateAppointmentPage(CreateAppointmentViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
