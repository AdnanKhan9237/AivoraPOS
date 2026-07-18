using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Enums;
using AivoraPOS.KeyGenerator.Entities;
using AivoraPOS.KeyGenerator.Services;

namespace AivoraPOS.KeyGenerator.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly KeyGenerationService _keyGenerationService;

    public MainViewModel(KeyGenerationService keyGenerationService)
    {
        _keyGenerationService = keyGenerationService;
        Plans = new ObservableCollection<LicensePlan>(
            Enum.GetValues<LicensePlan>().Cast<LicensePlan>());
        SelectedPlan = LicensePlan.Professional;
        ExpiryDate = DateTime.Today.AddYears(1);
        _ = LoadHistoryAsync();
    }

    public ObservableCollection<LicensePlan> Plans { get; }
    public ObservableCollection<GeneratedLicenseKey> History { get; } = new();

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private LicensePlan _selectedPlan;

    [ObservableProperty]
    private DateTime? _expiryDate;

    [ObservableProperty]
    private string _generatedKey = string.Empty;

    [RelayCommand]
    private async Task GenerateAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            MessageBox.Show("Customer name is required.", "AivoraPOS Key Generator", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (ExpiryDate is null)
        {
            MessageBox.Show("Expiry date is required.", "AivoraPOS Key Generator", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var record = await _keyGenerationService.GenerateAsync(CustomerName, SelectedPlan, ExpiryDate.Value);
        GeneratedKey = record.LicenseKey;
        await LoadHistoryAsync();
    }

    [RelayCommand]
    private void Copy()
    {
        if (!string.IsNullOrWhiteSpace(GeneratedKey))
        {
            Clipboard.SetText(GeneratedKey);
        }
    }

    private async Task LoadHistoryAsync()
    {
        var items = await _keyGenerationService.GetHistoryAsync();
        History.Clear();
        foreach (var item in items)
        {
            History.Add(item);
        }
    }
}
