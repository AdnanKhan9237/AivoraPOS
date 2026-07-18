using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Interfaces.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SetThemeAsync(AppTheme theme, CancellationToken cancellationToken = default);
    event EventHandler<AppTheme>? ThemeChanged;
}
