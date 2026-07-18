using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SetThemeAsync(AppTheme theme, CancellationToken cancellationToken = default);
    event EventHandler<AppTheme>? ThemeChanged;
}
