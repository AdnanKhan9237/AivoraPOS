using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;

namespace NovaPOS.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;

    public MainViewModel(ILicenseService licenseService)
    {
        _licenseService = licenseService;
        RefreshLicenseStatus();
    }

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _licenseStatusText = string.Empty;

    [ObservableProperty]
    private bool _showTrialBanner;

    public void RefreshLicenseStatus()
    {
        StatusMessage = "Ready";

        LicenseStatusText = _licenseService.CurrentStatus switch
        {
            LicenseStatus.Trial => $"Trial: {_licenseService.MaxProducts} products max • Watermarked receipts",
            LicenseStatus.GracePeriod => $"Offline grace period • {_licenseService.EffectivePlan} plan",
            LicenseStatus.Valid => $"Licensed • {_licenseService.EffectivePlan} plan",
            LicenseStatus.Expired => "License expired • Read-only mode",
            _ => "License status unknown"
        };

        ShowTrialBanner = _licenseService.IsTrial;
    }

    [RelayCommand]
    private void ActivateLicense()
    {
        OnActivateLicenseRequested?.Invoke();
    }

    public event Action? OnActivateLicenseRequested;
}
