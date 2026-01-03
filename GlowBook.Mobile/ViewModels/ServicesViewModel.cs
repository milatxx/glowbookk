using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlowBook.Mobile.Services;
using GlowBook.Model.Entities;
using System.Collections.ObjectModel;

namespace GlowBook.Mobile.ViewModels;

public partial class ServicesViewModel : BaseViewModel
{
    private readonly ApiClient _api;

    public ObservableCollection<Service> Services { get; } = new();

    [ObservableProperty] private string error = "";

    public ServicesViewModel(ApiClient api)
    {
        _api = api;
        Title = "Services";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        Error = "";

        try
        {
            var items = await _api.GetServicesAsync();
            Services.Clear();
            foreach (var s in items) Services.Add(s);
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
