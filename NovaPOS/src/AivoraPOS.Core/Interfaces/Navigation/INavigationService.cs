namespace AivoraPOS.Core.Interfaces.Navigation;

public interface INavigationService
{
    object? CurrentViewModel { get; }
    Type? CurrentViewModelType { get; }
    event EventHandler? CurrentViewModelChanged;
    Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class;
    Task<bool> NavigateToAsync(Type viewModelType);
}
