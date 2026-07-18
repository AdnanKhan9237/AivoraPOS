using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Session;

public sealed class SessionTimeoutService : ISessionTimeoutService, IDisposable
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionTimeoutService> _logger;
    private readonly Timer _timer;
    private DateTime _lastActivityUtc = DateTime.UtcNow;

    public SessionTimeoutService(
        ICurrentUserService currentUserService,
        IServiceScopeFactory scopeFactory,
        ILogger<SessionTimeoutService> logger)
    {
        _currentUserService = currentUserService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _timer = new Timer(OnTimerTick, null, Timeout.Infinite, Timeout.Infinite);
        IdleTimeout = TimeSpan.FromMinutes(5);
    }

    public TimeSpan IdleTimeout { get; set; }
    public event EventHandler? SessionTimedOut;

    public void RecordActivity() => _lastActivityUtc = DateTime.UtcNow;

    public void Start()
    {
        _lastActivityUtc = DateTime.UtcNow;
        _timer.Change(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
    }

    public void Stop() => _timer.Change(Timeout.Infinite, Timeout.Infinite);

    private async void OnTimerTick(object? state)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return;
        }

        if (DateTime.UtcNow - _lastActivityUtc < IdleTimeout)
        {
            return;
        }

        _logger.LogInformation("Session timed out due to inactivity.");

        using var scope = _scopeFactory.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await authService.LogoutAsync();

        SessionTimedOut?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose() => _timer.Dispose();
}
