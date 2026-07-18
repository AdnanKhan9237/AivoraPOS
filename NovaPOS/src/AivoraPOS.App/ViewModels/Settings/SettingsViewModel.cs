using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AivoraPOS.App.Views.About;
using AivoraPOS.App.Views.Settings;
using AivoraPOS.Core;
using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Licensing;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Settings;
using AivoraPOS.Licensing.Extensions;

namespace AivoraPOS.App.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly ISessionTimeoutService _sessionTimeoutService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserAdminService _userAdminService;
    private readonly IDataManagementService _dataManagementService;
    private readonly ILicenseService _licenseService;

    public SettingsViewModel(
        ISettingsService settingsService,
        IThemeService themeService,
        ISessionTimeoutService sessionTimeoutService,
        IAuthorizationService authorizationService,
        ICurrentUserService currentUserService,
        IUserAdminService userAdminService,
        IDataManagementService dataManagementService,
        ILicenseService licenseService)
    {
        _settingsService = settingsService;
        _themeService = themeService;
        _sessionTimeoutService = sessionTimeoutService;
        _authorizationService = authorizationService;
        _currentUserService = currentUserService;
        _userAdminService = userAdminService;
        _dataManagementService = dataManagementService;
        _licenseService = licenseService;

        CanManageUsers = _authorizationService.HasPermission(Permission.ManageUsers);
        CanManageData = _authorizationService.HasPermission(Permission.ManageSettings);
        CanExportData = _licenseService.CanUse(LicenseFeature.ExportPdfExcel);

        AppVersionText = $"{ProductInfo.AppName}  v{AppVersion.Current}          {ProductInfo.CopyrightShort}";

        CurrencyPositions = [CurrencyPosition.Before, CurrencyPosition.After];
        ReceiptWidths = [ReceiptWidth.Mm58, ReceiptWidth.Mm80, ReceiptWidth.A4];
        PaymentMethods = [PaymentMethod.Cash, PaymentMethod.Card, PaymentMethod.Mixed];
        IdleTimeoutOptions = [0, 1, 2, 5, 10, 15, 30];

        LoadPrinters();
        _ = LoadAsync();
    }

    public bool CanManageUsers { get; }
    public bool CanManageData { get; }
    public bool CanExportData { get; }
    public string AppVersionText { get; }

    public IReadOnlyList<CurrencyPosition> CurrencyPositions { get; }
    public IReadOnlyList<ReceiptWidth> ReceiptWidths { get; }
    public IReadOnlyList<PaymentMethod> PaymentMethods { get; }
    public IReadOnlyList<int> IdleTimeoutOptions { get; }
    public IReadOnlyList<int> AuditRetentionMonthOptions { get; } = [3, 6, 12, 24];

    public ObservableCollection<string> InstalledPrinters { get; } = [];
    public ObservableCollection<UserListItemVm> Users { get; } = [];

    [ObservableProperty]
    private int _selectedSectionIndex;

    [ObservableProperty]
    private string? _statusMessage;

    // Business
    [ObservableProperty] private string _businessName = string.Empty;
    [ObservableProperty] private string _businessAddress = string.Empty;
    [ObservableProperty] private string _businessPhone = string.Empty;
    [ObservableProperty] private string? _logoPath;
    [ObservableProperty] private string _currencySymbol = "$";
    [ObservableProperty] private CurrencyPosition _currencyPosition = CurrencyPosition.Before;
    [ObservableProperty] private decimal _defaultTaxRatePercent = 8.25m;

    // Receipt
    [ObservableProperty] private string _receiptHeader = string.Empty;
    [ObservableProperty] private string _receiptFooter = string.Empty;
    [ObservableProperty] private bool _showLogoOnReceipt;
    [ObservableProperty] private bool _autoPrintReceipt;
    [ObservableProperty] private string _selectedPrinter = string.Empty;
    [ObservableProperty] private ReceiptWidth _receiptWidth = ReceiptWidth.Mm80;

    // POS
    [ObservableProperty] private int _idleLockTimeoutMinutes = 5;
    [ObservableProperty] private bool _requireManagerForDiscount;
    [ObservableProperty] private bool _allowNegativeStock;
    [ObservableProperty] private PaymentMethod _defaultPaymentMethod = PaymentMethod.Cash;
    [ObservableProperty] private bool _soundOnSaleComplete = true;
    [ObservableProperty] private bool _isDarkTheme;

    // License
    [ObservableProperty] private string _licensePlanName = string.Empty;
    [ObservableProperty] private string _licenseExpiryText = string.Empty;
    [ObservableProperty] private string _machineId = string.Empty;
    [ObservableProperty] private string _licensedBusinessName = string.Empty;
    [ObservableProperty] private string _licenseKeyInput = string.Empty;
    [ObservableProperty] private string _licenseBusinessNameInput = string.Empty;

    // Data
    [ObservableProperty] private int _selectedAuditRetentionMonths = 12;

    [ObservableProperty]
    private UserListItemVm? _selectedUser;

    [RelayCommand]
    private async Task LoadAsync()
    {
        var business = await _settingsService.GetBusinessInfoAsync();
        BusinessName = business.Name;
        BusinessAddress = business.Address;
        BusinessPhone = business.Phone;
        LogoPath = business.LogoPath;
        CurrencySymbol = business.CurrencySymbol;
        CurrencyPosition = business.CurrencyPosition;
        DefaultTaxRatePercent = business.DefaultTaxRate * 100m;

        var receipt = await _settingsService.GetReceiptSettingsAsync();
        ReceiptHeader = receipt.HeaderText;
        ReceiptFooter = receipt.FooterText;
        ShowLogoOnReceipt = receipt.ShowLogo;
        AutoPrintReceipt = receipt.AutoPrint;
        SelectedPrinter = receipt.PrinterName;
        ReceiptWidth = receipt.Width;

        var pos = await _settingsService.GetPosBehaviorAsync();
        IdleLockTimeoutMinutes = pos.IdleLockTimeoutMinutes;
        RequireManagerForDiscount = pos.RequireManagerForDiscount;
        AllowNegativeStock = pos.AllowNegativeStock;
        DefaultPaymentMethod = pos.DefaultPaymentMethod;
        SoundOnSaleComplete = pos.SoundOnSaleComplete;
        ApplyIdleTimeout(pos.IdleLockTimeoutMinutes);

        IsDarkTheme = _themeService.CurrentTheme == AppTheme.Dark;
        await LoadLicenseDetailsAsync();
        if (CanManageUsers)
        {
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task SaveBusinessAsync()
    {
        await _settingsService.SaveBusinessInfoAsync(new BusinessInfoDto
        {
            Name = BusinessName,
            Address = BusinessAddress,
            Phone = BusinessPhone,
            LogoPath = LogoPath,
            CurrencySymbol = CurrencySymbol,
            CurrencyPosition = CurrencyPosition,
            DefaultTaxRate = DefaultTaxRatePercent / 100m
        });
        StatusMessage = "Business information saved.";
    }

    [RelayCommand]
    private async Task PickLogoAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All files|*.*"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        AppPaths.EnsureDirectoriesExist();
        File.Copy(dialog.FileName, AppPaths.LogoFilePath, overwrite: true);
        LogoPath = AppPaths.LogoFilePath;
        await _settingsService.SetAsync(SettingKeys.StoreLogoPath, LogoPath);
        StatusMessage = "Logo updated.";
    }

    [RelayCommand]
    private async Task SaveReceiptAsync()
    {
        await _settingsService.SaveReceiptSettingsAsync(new ReceiptSettingsDto
        {
            HeaderText = ReceiptHeader,
            FooterText = ReceiptFooter,
            ShowLogo = ShowLogoOnReceipt,
            AutoPrint = AutoPrintReceipt,
            PrinterName = SelectedPrinter,
            Width = ReceiptWidth
        });
        StatusMessage = "Receipt settings saved.";
    }

    [RelayCommand]
    private async Task SavePosBehaviorAsync()
    {
        await _settingsService.SavePosBehaviorAsync(new PosBehaviorDto
        {
            IdleLockTimeoutMinutes = IdleLockTimeoutMinutes,
            RequireManagerForDiscount = RequireManagerForDiscount,
            AllowNegativeStock = AllowNegativeStock,
            DefaultPaymentMethod = DefaultPaymentMethod,
            SoundOnSaleComplete = SoundOnSaleComplete
        });
        ApplyIdleTimeout(IdleLockTimeoutMinutes);
        StatusMessage = "POS behavior saved.";
    }

    partial void OnIsDarkThemeChanged(bool value) => _ = ApplyThemeAsync();

    private async Task ApplyThemeAsync()
    {
        await _themeService.SetThemeAsync(IsDarkTheme ? AppTheme.Dark : AppTheme.Light);
        await _settingsService.SetAsync(SettingKeys.UiTheme, IsDarkTheme ? AppTheme.Dark : AppTheme.Light);
    }

    [RelayCommand]
    private async Task ActivateLicenseAsync()
    {
        if (string.IsNullOrWhiteSpace(LicenseKeyInput) || string.IsNullOrWhiteSpace(LicenseBusinessNameInput))
        {
            StatusMessage = "License key and business name are required.";
            return;
        }

        var result = await _licenseService.ActivateAsync(LicenseKeyInput, LicenseBusinessNameInput);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return;
        }

        await _licenseService.ValidateOnLaunchAsync();
        await LoadLicenseDetailsAsync();
        StatusMessage = "License activated successfully.";
    }

    [RelayCommand]
    private async Task TransferLicenseAsync()
    {
        var confirm = MessageBox.Show(
            "This will deactivate the license on this machine. Continue?",
            "Transfer License",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var transferred = await _licenseService.TransferLicenseAsync();
        StatusMessage = transferred
            ? "License removed from this machine. Activate on the new machine."
            : "No active license found on this machine.";
        await LoadLicenseDetailsAsync();
    }

    [RelayCommand]
    private void OpenPurchasePage()
    {
        StatusMessage = "Online purchase is coming soon. Contact support to activate your license.";
    }

    [RelayCommand]
    private async Task BackupDatabaseAsync()
    {
        if (!CanManageData)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "Database backup|*.db|All files|*.*",
            FileName = $"aivorapos-backup-{DateTime.Now:yyyyMMdd-HHmm}.db"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        await _dataManagementService.BackupDatabaseAsync(dialog.FileName);
        StatusMessage = $"Database backed up to {dialog.FileName}";
    }

    [RelayCommand]
    private async Task RestoreDatabaseAsync()
    {
        if (!CanManageData)
        {
            return;
        }

        var dialog = new OpenFileDialog { Filter = "Database backup|*.db|All files|*.*" };
        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var confirm = MessageBox.Show(
            "Restoring will replace the current database. Restart AivoraPOS after restore. Continue?",
            "Restore Database",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        await _dataManagementService.RestoreDatabaseAsync(dialog.FileName);
        StatusMessage = "Database restored. Please restart AivoraPOS.";
    }

    [RelayCommand]
    private async Task ExportDataCsvAsync()
    {
        try
        {
            _licenseService.RequireFeature(LicenseFeature.ExportPdfExcel);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV files|*.csv",
            FileName = $"aivorapos-export-{DateTime.Now:yyyyMMdd}.csv"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var path = await _dataManagementService.ExportAllDataCsvAsync(dialog.FileName);
        StatusMessage = $"Data exported to {path}";
    }

    [RelayCommand]
    private async Task ClearOldLogsAsync()
    {
        if (!CanManageData)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Delete audit logs older than {SelectedAuditRetentionMonths} months?",
            "Clear Old Logs",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var deleted = await _dataManagementService.ClearAuditLogsOlderThanAsync(SelectedAuditRetentionMonths);
        StatusMessage = $"Deleted {deleted} audit log entries.";
    }

    [RelayCommand]
    private async Task AddUserAsync()
    {
        var saved = await ShowUserEditDialogAsync(null);
        if (saved)
        {
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task EditUserAsync(UserListItemVm? user)
    {
        if (user is null)
        {
            return;
        }

        var saved = await ShowUserEditDialogAsync(user);
        if (saved)
        {
            await LoadUsersAsync();
        }
    }

    [RelayCommand]
    private async Task ResetPinAsync(UserListItemVm? user)
    {
        if (user is null)
        {
            return;
        }

        var pin = PromptPin($"Enter new 4-digit PIN for {user.FullName}");
        if (pin is null || pin.Length != 4 || !pin.All(char.IsDigit))
        {
            StatusMessage = "PIN must be exactly 4 digits.";
            return;
        }

        await _userAdminService.ResetPinAsync(user.Id, pin);
        StatusMessage = $"PIN reset for {user.FullName}.";
    }

    [RelayCommand]
    private async Task DeactivateUserAsync(UserListItemVm? user)
    {
        if (user is null || !user.IsActive)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Deactivate {user.FullName}?",
            "Deactivate User",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        await _userAdminService.DeactivateUserAsync(user.Id);
        await LoadUsersAsync();
        StatusMessage = $"{user.FullName} deactivated.";
    }

    [RelayCommand]
    private async Task ChangeOwnPasswordAsync()
    {
        var userId = _currentUserService.CurrentUser?.Id;
        if (userId is null)
        {
            return;
        }

        var result = await ShowChangePasswordDialogAsync();
        if (result is null)
        {
            return;
        }

        try
        {
            await _userAdminService.ChangeOwnPasswordAsync(userId.Value, result.Value.Current, result.Value.New);
            StatusMessage = "Password changed successfully.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task LoadUsersAsync()
    {
        var users = await _userAdminService.GetAllUsersAsync();
        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(new UserListItemVm(user.Id, user.FullName, user.Role.ToString(), user.IsActive, user.LastLoginAt));
        }
    }

    private async Task LoadLicenseDetailsAsync()
    {
        var details = await _licenseService.GetLicenseDetailsAsync();
        LicensePlanName = details.PlanName;
        LicenseExpiryText = details.ExpiresAt?.ToString("MMMM d, yyyy") ?? "N/A";
        MachineId = details.MachineId;
        LicensedBusinessName = details.LicensedBusinessName ?? "Not activated";
    }

    private void ApplyIdleTimeout(int minutes)
    {
        _sessionTimeoutService.IdleTimeout = minutes <= 0
            ? TimeSpan.FromDays(365)
            : TimeSpan.FromMinutes(minutes);
    }

    private void LoadPrinters()
    {
        InstalledPrinters.Clear();
        try
        {
            var server = new System.Printing.LocalPrintServer();
            foreach (var queue in server.GetPrintQueues().OrderBy(x => x.Name))
            {
                InstalledPrinters.Add(queue.Name);
            }
        }
        catch
        {
            // No printers available on this platform.
        }

        if (InstalledPrinters.Count == 0)
        {
            InstalledPrinters.Add("(Default printer)");
        }
    }

    private async Task<bool> ShowUserEditDialogAsync(UserListItemVm? existing)
    {
        var tcs = new TaskCompletionSource<bool>();
        UserEditViewModel vm;

        if (existing is null)
        {
            vm = new UserEditViewModel(null, null, null, saved => tcs.TrySetResult(saved));
        }
        else
        {
            vm = new UserEditViewModel(
                existing.Id,
                existing.FullName,
                Enum.Parse<UserRole>(existing.Role),
                saved => tcs.TrySetResult(saved));
        }

        var window = new UserEditWindow { DataContext = vm, Owner = Application.Current.MainWindow };
        window.ShowDialog();

        if (!await tcs.Task)
        {
            return false;
        }

        try
        {
            if (existing is null)
            {
                if (string.IsNullOrWhiteSpace(vm.FullName) || string.IsNullOrWhiteSpace(vm.Username))
                {
                    StatusMessage = "Name and username are required.";
                    return false;
                }

                if (vm.Pin.Length != 4 || vm.Password.Length < 4)
                {
                    StatusMessage = "PIN (4 digits) and password are required for new users.";
                    return false;
                }

                await _userAdminService.CreateUserAsync(vm.FullName, vm.Username, vm.SelectedRole, vm.Pin, vm.Password);
            }
            else
            {
                await _userAdminService.UpdateUserAsync(existing.Id, vm.FullName, vm.SelectedRole, existing.IsActive);
            }

            return true;
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            return false;
        }
    }

    private Task<(string Current, string New)?> ShowChangePasswordDialogAsync()
    {
        var tcs = new TaskCompletionSource<(string Current, string New)?>();
        var vm = new ChangePasswordViewModel(result => tcs.TrySetResult(result));
        var window = new ChangePasswordWindow { DataContext = vm, Owner = Application.Current.MainWindow };
        window.ShowDialog();
        return tcs.Task;
    }

    private static string? PromptPin(string prompt)
    {
        var window = new ResetPinWindow(prompt) { Owner = Application.Current.MainWindow };
        return window.ShowDialog() == true ? window.Pin : null;
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var dialog = new AboutDialog
        {
            Owner = Application.Current.MainWindow
        };
        dialog.ShowDialog();
    }
}
