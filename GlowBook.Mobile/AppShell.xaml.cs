using GlowBook.Mobile.Views;

namespace GlowBook.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("appointment/new", typeof(CreateAppointmentPage));
    }
}
