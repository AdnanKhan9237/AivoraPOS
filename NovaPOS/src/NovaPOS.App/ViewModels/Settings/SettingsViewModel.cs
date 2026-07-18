using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IAppSettingRepository _appSettingRepository;
    private readonly ISessionTimeoutService _sessionTimeoutService;
    private readonly IAuthorizationService _authorizationService;

    public SettingsViewModel(
        IThemeService themeService,
        IAppSettingRepository appSettingRepository,
        ISessionTimeoutService sessionTimeoutService,
        IAuthorizationService authorizationService)
    {
        _themeService = themeService;
        _appSettingRepository = appSettingRepository;
        _sessionTimeoutService = sessionTimeoutService;
        _authorizationService = authorizationService;
        IsDarkTheme = _themeService.CurrentTheme == AppTheme.Dark;
        CanManageSession = _authorizationService.HasPermission(Permission.ManageSettings);
        _ = LoadAsync();
    }

    public bool CanManageSession { get; }

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private int _idleTimeoutMinutes = 5;

    [ObservableProperty]
    private string? _statusMessage;

    [RelayCommand]
    private async Task ApplyThemeAsync()
    {
        var theme = IsDarkTheme ? AppTheme.Dark : AppTheme.Light;
        await _themeService.SetThemeAsync(theme);
        StatusMessage = $"Theme set to {theme}.";
    }

    partial void OnIsDarkThemeChanged(bool value) => _ = ApplyThemeAsync();

    [RelayCommand]
    private async Task SaveSessionTimeoutAsync()
    {
        if (!CanManageSession)
        {
            return;
        }

        if (IdleTimeoutMinutes < 1)
        {
            StatusMessage = "Idle timeout must be at least 1 minute.";
            return;
        }

        var setting = await _appSettingRepository.GetByKeyAsync("Session.IdleTimeoutMinutes");
        if (setting is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = "Session.IdleTimeoutMinutes",
                Value = IdleTimeoutMinutes.ToString()
            });
        }
        else
        {
            setting.Value = IdleTimeoutMinutes.ToString();
            await _appSettingRepository.UpdateAsync(setting);
        }

        await _appSettingRepository.SaveChangesAsync();
        _sessionTimeoutService.IdleTimeout = TimeSpan.FromMinutes(IdleTimeoutMinutes);
        StatusMessage = "Session settings saved.";
    }

    private async Task LoadAsync()
    {
        var setting = await _appSettingRepository.GetByKeyAsync("Session.IdleTimeoutMinutes");
        if (setting is not null && int.TryParse(setting.Value, out var minutes) && minutes > 0)
        {
            IdleTimeoutMinutes = minutes;
            _sessionTimeoutService.IdleTimeout = TimeSpan.FromMinutes(minutes);
        }
    }
}
