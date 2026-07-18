using System.Windows;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.Services;

public sealed class ThemeService : IThemeService
{
    private const string ThemeSettingKey = "UI.Theme";
    private readonly IAppSettingRepository _appSettingRepository;
    private ResourceDictionary? _currentThemeDictionary;

    public ThemeService(IAppSettingRepository appSettingRepository)
    {
        _appSettingRepository = appSettingRepository;
    }

    public AppTheme CurrentTheme { get; private set; } = AppTheme.Light;

    public event EventHandler<AppTheme>? ThemeChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _appSettingRepository.GetByKeyAsync(ThemeSettingKey, cancellationToken);
        var theme = ParseTheme(setting?.Value);
        ApplyTheme(theme, persist: false);
    }

    public async Task SetThemeAsync(AppTheme theme, CancellationToken cancellationToken = default)
    {
        ApplyTheme(theme, persist: true);

        var setting = await _appSettingRepository.GetByKeyAsync(ThemeSettingKey, cancellationToken);
        if (setting is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = ThemeSettingKey,
                Value = theme.ToString()
            }, cancellationToken);
        }
        else
        {
            setting.Value = theme.ToString();
            await _appSettingRepository.UpdateAsync(setting, cancellationToken);
        }

        await _appSettingRepository.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTheme(AppTheme theme, bool persist)
    {
        if (!persist && _currentThemeDictionary is not null && theme == CurrentTheme)
        {
            return;
        }

        var app = Application.Current;
        if (app is null)
        {
            CurrentTheme = theme;
            return;
        }

        var dictionaries = app.Resources.MergedDictionaries;
        var existing = dictionaries.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase) == true);

        if (existing is not null)
        {
            dictionaries.Remove(existing);
        }

        if (_currentThemeDictionary is not null)
        {
            dictionaries.Remove(_currentThemeDictionary);
        }

        var source = theme == AppTheme.Dark
            ? new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            : new Uri("Themes/LightTheme.xaml", UriKind.Relative);

        _currentThemeDictionary = new ResourceDictionary { Source = source };
        app.Resources.MergedDictionaries.Insert(0, _currentThemeDictionary);
        CurrentTheme = theme;
        ThemeChanged?.Invoke(this, theme);
    }

    private static AppTheme ParseTheme(string? value) =>
        Enum.TryParse<AppTheme>(value, true, out var theme) ? theme : AppTheme.Light;
}
