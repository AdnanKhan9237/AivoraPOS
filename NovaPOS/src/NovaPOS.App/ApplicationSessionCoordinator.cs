namespace NovaPOS.App;

public sealed class ApplicationSessionCoordinator
{
    public event Action? LockScreenRequested;

    public void RequestLockScreen() => LockScreenRequested?.Invoke();
}
