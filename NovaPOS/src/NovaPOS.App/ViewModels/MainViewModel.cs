using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Enums;
using NovaPOS.App.ViewModels.Sales;
using NovaPOS.App.ViewModels.Reports;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILicenseService _licenseService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Action<AuditLogViewModel> _showAuditLog;
    private readonly Action _requestLockScreen;

    public MainViewModel(
        ILicenseService licenseService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IServiceScopeFactory scopeFactory,
        SalesViewModel salesViewModel,
        Action<AuditLogViewModel> showAuditLog,
        Action requestLockScreen)
    {
        _licenseService = licenseService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _scopeFactory = scopeFactory;
        Sales = salesViewModel;
        _showAuditLog = showAuditLog;
        _requestLockScreen = requestLockScreen;
        RefreshLicenseStatus();
        RefreshUserStatus();
        _currentUserService.CurrentUserChanged += (_, _) => RefreshUserStatus();
    }

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _licenseStatusText = string.Empty;

    [ObservableProperty]
    private string _userStatusText = string.Empty;

    [ObservableProperty]
    private bool _showTrialBanner;

    [ObservableProperty]
    private bool _canViewAuditLog;

    public SalesViewModel Sales { get; }

    [ObservableProperty]
    private bool _canViewReports;

    public void RefreshLicenseStatus()
    {
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

    public void RefreshUserStatus()
    {
        UserStatusText = _currentUserService.CurrentUser is null
            ? "Not signed in"
            : $"{_currentUserService.CurrentUser.FullName} ({_currentUserService.CurrentUser.Role})";

        CanViewAuditLog = _authorizationService.HasPermission(Permission.ViewAuditLog);
        CanViewReports = _authorizationService.HasPermission(Permission.ViewReports);
    }

    [RelayCommand]
    private void OpenReports()
    {
        using var scope = _scopeFactory.CreateScope();
        var vm = scope.ServiceProvider.GetRequiredService<ReportsViewModel>();

        if (!Behaviors.AuthorizationBehavior.CanNavigateTo(vm, _authorizationService))
        {
            return;
        }

        vm.UpgradeRequested += () => OnActivateLicenseRequested?.Invoke();
        var window = new Views.Reports.ReportsWindow { DataContext = vm, Owner = System.Windows.Application.Current.MainWindow };
        window.ShowDialog();
        vm.UpgradeRequested -= () => OnActivateLicenseRequested?.Invoke();
    }

    [RelayCommand]
    private void ActivateLicense() => OnActivateLicenseRequested?.Invoke();

    [RelayCommand]
    private void OpenAuditLog()
    {
        using var scope = _scopeFactory.CreateScope();
        var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var vm = new AuditLogViewModel(auditLogRepository, userRepository);

        if (!Behaviors.AuthorizationBehavior.CanNavigateTo(vm, _authorizationService))
        {
            return;
        }

        _showAuditLog(vm);
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.LogoutAsync();
        _requestLockScreen();
    }

    public event Action? OnActivateLicenseRequested;
}
