namespace NovaPOS.Core.Interfaces.Navigation;

public interface INavigationGuard
{
    Task<bool> CanNavigateAwayAsync();
}
