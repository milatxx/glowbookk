using System.Collections.ObjectModel;
using GlowBook.Model.Entities;
using GlowBook.Mobile.Services;

namespace GlowBook.Mobile.ViewModels;

public class ServicesViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<Service> Services { get; } = new();

    public ServicesViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        Title = "Services";
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Services.Clear();

            var items = await _apiClient.GetServicesAsync();

            foreach (var service in items)
            {
                Services.Add(service);
            }
        }
        catch (Exception ex)
        {
            // logging
            System.Diagnostics.Debug.WriteLine($"Failed to load services: {ex}");
            throw; // Page kan DisplayAlert tonen
        }
        finally
        {
            IsBusy = false;
        }
    }
}
