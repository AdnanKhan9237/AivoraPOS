using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Models;

namespace NovaPOS.App.ViewModels;

public partial class ActivateLicenseViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly Action<bool> _closeAction;

    public ActivateLicenseViewModel(ILicenseService licenseService, Action<bool> closeAction)
    {
        _licenseService = licenseService;
        _closeAction = closeAction;
    }

    [ObservableProperty]
    private string _businessName = string.Empty;

    [ObservableProperty]
    private string _licenseKey = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task ActivateAsync()
    {
        StatusMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(BusinessName) || string.IsNullOrWhiteSpace(LicenseKey))
        {
            StatusMessage = "Business name and license key are required.";
            return;
        }

        var result = await _licenseService.ActivateAsync(LicenseKey, BusinessName);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return;
        }

        _closeAction(true);
    }

    [RelayCommand]
    private void Cancel() => _closeAction(false);
}
