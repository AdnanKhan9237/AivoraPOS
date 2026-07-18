using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.App.Services;
using NovaPOS.App.ViewModels.Products;
using NovaPOS.App.ViewModels.Reports;
using NovaPOS.App.ViewModels.Sales;
using NovaPOS.App.ViewModels.Settings;
using NovaPOS.App.ViewModels.Shell;
using NovaPOS.App.ViewModels.Users;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Navigation;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ILicenseService _licenseService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly INavigationService _navigationService;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly IReportService _reportService;
    private readonly ISettingsService _settingsService;
    private readonly ISessionTimeoutService _sessionTimeoutService;
    private readonly IAuthService _authService;
    private readonly UpdateCoordinator _updateCoordinator;
    private readonly ReportsViewModel _reportsViewModel;
    private readonly DispatcherTimer _statusTimer;

    public MainViewModel(
        ILicenseService licenseService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        INavigationService navigationService,
        IInventoryAlertService inventoryAlertService,
        IReportService reportService,
        ISettingsService settingsService,
        ISessionTimeoutService sessionTimeoutService,
        IAuthService authService,
        UpdateCoordinator updateCoordinator,
        IUserRepository userRepository,
        ReportsViewModel reportsViewModel,
        LockOverlayViewModel lockOverlayViewModel)
    {
        _licenseService = licenseService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _navigationService = navigationService;
        _inventoryAlertService = inventoryAlertService;
        _reportService = reportService;
        _settingsService = settingsService;
        _sessionTimeoutService = sessionTimeoutService;
        _authService = authService;
        _updateCoordinator = updateCoordinator;
        _reportsViewModel = reportsViewModel;
        LockOverlay = lockOverlayViewModel;
        LockOverlay.Unlocked += OnUnlocked;

        _navigationService.CurrentViewModelChanged += OnCurrentViewModelChanged;

        NavigationItems =
        [
            CreateNavItem("sales", "Sales", "🏪", typeof(SalesViewModel)),
            CreateNavItem("inventory", "Inventory", "📦", typeof(ProductsViewModel)),
            CreateNavItem("reports", "Reports", "📊", typeof(ReportsViewModel)),
            CreateNavItem("users", "Users", "👥", typeof(UsersViewModel)),
            CreateNavItem("settings", "Settings", "⚙", typeof(SettingsViewModel)),
            CreateNavItem("audit", "Audit Log", "🔒", typeof(AuditLogViewModel))
        ];

        _inventoryAlertService.LowStockCountChanged += (_, _) => UpdateInventoryNavBadge();
        _currentUserService.CurrentUserChanged += (_, _) => RefreshUserStatus();
        _sessionTimeoutService.SessionTimedOut += OnSessionTimedOut;

        _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _statusTimer.Tick += async (_, _) => await RefreshStatusBarAsync();

        RefreshLicenseStatus();
        RefreshUserStatus();
        UpdateNavigationVisibility();
        _ = InitializeAsync();
    }

    public LockOverlayViewModel LockOverlay { get; }

    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    public object? CurrentPage => _navigationService.CurrentViewModel;

    [ObservableProperty]
    private string _businessName = "NovaPOS Store";

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    [ObservableProperty]
    private string _licenseBannerText = string.Empty;

    [ObservableProperty]
    private bool _showLicenseBanner;

    [ObservableProperty]
    private string _connectionStatus = "Connected";

    [ObservableProperty]
    private string _todaySalesText = "Today: 0 sales $0.00";

    [ObservableProperty]
    private string _licensePillText = "Trial";

    [ObservableProperty]
    private string _lowStockStatusText = string.Empty;

    [ObservableProperty]
    private bool _showLowStockStatus;

    [ObservableProperty]
    private bool _showUpdateBanner;

    [ObservableProperty]
    private string _updateBannerText = string.Empty;

    [ObservableProperty]
    private bool _isLocked;

    public void ApplyUpdateState(UpdateCoordinator? updateCoordinator = null)
    {
        var coordinator = updateCoordinator ?? _updateCoordinator;
        ShowUpdateBanner = coordinator.HasUpdate;
        UpdateBannerText = coordinator.BannerText;
    }

    [RelayCommand]
    private async Task InstallUpdateAsync()
    {
        try
        {
            await _updateCoordinator.InstallAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "The update could not be downloaded. Please check your internet connection and try again later.",
                "Update Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Serilog.Log.Warning(ex, "Update installation failed.");
        }
    }

    [RelayCommand]
    private void DismissUpdate()
    {
        _updateCoordinator.Dismiss();
        ShowUpdateBanner = false;
        UpdateBannerText = string.Empty;
    }

    public event Action? OnActivateLicenseRequested;

    public void RefreshLicenseStatus()
    {
        LicenseBannerText = _licenseService.CurrentStatus switch
        {
            LicenseStatus.Trial => $"Trial: {_licenseService.TrialDaysRemaining ?? 0} day(s) remaining",
            LicenseStatus.Valid => _licenseService.ExpiresAt.HasValue
                ? $"{_licenseService.EffectivePlan} until {_licenseService.ExpiresAt.Value:MMM yyyy}"
                : $"{_licenseService.EffectivePlan} plan",
            LicenseStatus.GracePeriod => $"Offline grace • {_licenseService.EffectivePlan}",
            LicenseStatus.Expired => "License expired • Read-only mode",
            _ => "Activate a license to unlock all features"
        };

        ShowLicenseBanner = _licenseService.IsTrial || _licenseService.CurrentStatus is LicenseStatus.Expired or LicenseStatus.Invalid;
        LicensePillText = _licenseService.IsTrial
            ? "Trial"
            : _licenseService.EffectivePlan.ToString();
        UpdateNavigationVisibility();
    }

    public void RefreshUserStatus()
    {
        UserDisplayName = _currentUserService.CurrentUser is null
            ? "Not signed in"
            : _currentUserService.CurrentUser.FullName;
        UpdateNavigationVisibility();
    }

    public async Task RefreshInventoryAlertsAsync()
    {
        await _inventoryAlertService.RefreshAsync();
        UpdateInventoryNavBadge();
        await RefreshStatusBarAsync();
    }

    public void RequestLock(DateTime lockedAtLocal)
    {
        LockOverlay.SetLockedAt(lockedAtLocal);
        IsLocked = true;
        _sessionTimeoutService.Stop();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        RequestLock(DateTime.Now);
    }

    [RelayCommand]
    private void ActivateLicense() => OnActivateLicenseRequested?.Invoke();

    private void OnUnlocked()
    {
        IsLocked = false;
        RefreshUserStatus();
        _sessionTimeoutService.Start();
    }

    private void OnSessionTimedOut(object? sender, EventArgs e)
    {
        RequestLock(DateTime.Now);
    }

    private Type? _previousViewModelType;

    private void OnCurrentViewModelChanged(object? sender, EventArgs e)
    {
        if (_previousViewModelType == typeof(SettingsViewModel))
        {
            _ = ReloadBusinessNameAsync();
        }

        _previousViewModelType = _navigationService.CurrentViewModelType;
        OnPropertyChanged(nameof(CurrentPage));
        UpdateSelectedNavigation();
        _reportsViewModel.UpgradeRequested -= OnReportsUpgradeRequested;
        if (_navigationService.CurrentViewModel is ReportsViewModel)
        {
            _reportsViewModel.UpgradeRequested += OnReportsUpgradeRequested;
        }
    }

    private void OnReportsUpgradeRequested() => OnActivateLicenseRequested?.Invoke();

    private NavigationItemViewModel CreateNavItem(string key, string label, string icon, Type viewModelType) =>
        new(key, label, icon, viewModelType, async () => await NavigateToAsync(viewModelType));

    private async Task NavigateToAsync(Type viewModelType)
    {
        if (IsLocked)
        {
            return;
        }

        if (!CanAccess(viewModelType))
        {
            MessageBox.Show(
                "You do not have permission to access this screen.",
                "Access Denied",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (await _navigationService.NavigateToAsync(viewModelType))
        {
            UpdateSelectedNavigation();
        }
    }

    private bool CanAccess(Type viewModelType)
    {
        if (viewModelType == typeof(SalesViewModel))
        {
            return _authorizationService.HasPermission(Permission.ProcessSale);
        }

        if (viewModelType == typeof(ProductsViewModel))
        {
            return _authorizationService.HasPermission(Permission.ManageProducts);
        }

        if (viewModelType == typeof(ReportsViewModel))
        {
            return _authorizationService.HasPermission(Permission.ViewReports);
        }

        if (viewModelType == typeof(UsersViewModel))
        {
            return _authorizationService.HasPermission(Permission.ManageUsers);
        }

        if (viewModelType == typeof(AuditLogViewModel))
        {
            return _authorizationService.HasPermission(Permission.ViewAuditLog);
        }

        return true;
    }

    private void UpdateSelectedNavigation()
    {
        foreach (var item in NavigationItems)
        {
            item.IsSelected = item.ViewModelType == _navigationService.CurrentViewModelType;
        }
    }

    private void UpdateNavigationVisibility()
    {
        foreach (var item in NavigationItems)
        {
            item.IsVisible = CanAccess(item.ViewModelType);
            item.ShowLockedIcon = item.Key == "reports" && !_licenseService.CanUse(LicenseFeature.FullReports);
        }

        UpdateInventoryNavBadge();
    }

    private void UpdateInventoryNavBadge()
    {
        var inventory = NavigationItems.FirstOrDefault(x => x.Key == "inventory");
        if (inventory is null)
        {
            return;
        }

        var count = _inventoryAlertService.LowStockCount;
        inventory.ShowBadge = count > 0 && inventory.IsVisible;
        inventory.BadgeText = count > 0 ? count.ToString() : string.Empty;
    }

    private async Task InitializeAsync()
    {
        await ReloadBusinessNameAsync();
        await RefreshInventoryAlertsAsync();
        await _navigationService.NavigateToAsync<SalesViewModel>();
        UpdateSelectedNavigation();

        _statusTimer.Start();
        await RefreshStatusBarAsync();
    }

    private async Task ReloadBusinessNameAsync()
    {
        var business = await _settingsService.GetBusinessInfoAsync();
        if (!string.IsNullOrWhiteSpace(business.Name))
        {
            BusinessName = business.Name;
        }
    }

    private async Task RefreshStatusBarAsync()
    {
        try
        {
            var summary = await _reportService.GetDailySummaryAsync(DateTime.Today);
            TodaySalesText = $"Today: {summary.TotalTransactions} sales {summary.TotalRevenue:C}";
        }
        catch
        {
            TodaySalesText = "Today: —";
        }

        ConnectionStatus = "Connected";
        ShowLowStockStatus = _inventoryAlertService.LowStockCount > 0 &&
                             _authorizationService.HasPermission(Permission.ManageProducts);
        LowStockStatusText = ShowLowStockStatus
            ? $"Low stock: {_inventoryAlertService.LowStockCount}"
            : string.Empty;
    }

    public void Dispose()
    {
        _statusTimer.Stop();
        _sessionTimeoutService.SessionTimedOut -= OnSessionTimedOut;
        _reportsViewModel.UpgradeRequested -= OnReportsUpgradeRequested;
    }
}
