using System.Collections.ObjectModel;
using GlowBook.Model.Entities;
using GlowBook.Mobile.Services;
using CommunityToolkit.Mvvm.Input;

namespace GlowBook.Mobile.ViewModels;

public partial class CustomersViewModel : BaseViewModel
{
    private readonly ApiClient _apiClient;

    public ObservableCollection<Customer> Customers { get; } = new();
    public ObservableCollection<Customer> FilteredCustomers { get; } = new();

    private string _searchText = string.Empty;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                PasFilterToe();
            }
        }
    }

    public CustomersViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        Title = "Klanten";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;

            Customers.Clear();

            var items = await _apiClient.GetCustomersAsync();
            foreach (var c in items)
                Customers.Add(c);

            PasFilterToe();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Klanten laden mislukt: {ex}");
            throw;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void PasFilterToe()
    {
        var q = (SearchText ?? "").Trim().ToLowerInvariant();

        FilteredCustomers.Clear();

        foreach (var c in Customers)
        {
            var hay = $"{c.Name} {c.Email} {c.Phone}".ToLowerInvariant();

            if (string.IsNullOrEmpty(q) || hay.Contains(q))
                FilteredCustomers.Add(c);
        }
    }
}
