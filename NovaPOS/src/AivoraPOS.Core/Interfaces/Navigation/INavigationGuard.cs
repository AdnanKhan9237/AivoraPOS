namespace AivoraPOS.Core.Interfaces.Navigation;

public interface INavigationGuard
{
    Task<bool> CanNavigateAwayAsync();
}
