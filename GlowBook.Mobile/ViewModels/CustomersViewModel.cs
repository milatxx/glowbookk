using System.Collections.ObjectModel;
using GlowBook.Model.Entities;
using GlowBook.Mobile.Services;

namespace GlowBook.Mobile.ViewModels;

public class CustomersViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<Customer> Customers { get; } = new();

    public CustomersViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        Title = "Customers";
    }

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            Customers.Clear();

            var items = await _apiClient.GetCustomersAsync();

            foreach (var customer in items)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            // logging
            System.Diagnostics.Debug.WriteLine($"Failed to load customers: {ex}");
            throw; // Page kan DisplayAlert tonen
        }
        finally
        {
            IsBusy = false;
        }
    }
}
