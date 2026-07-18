using System.Windows;
using AivoraPOS.Core.Interfaces.Navigation;

namespace AivoraPOS.App.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? CurrentViewModel { get; private set; }
    public Type? CurrentViewModelType { get; private set; }

    public event EventHandler? CurrentViewModelChanged;

    public Task<bool> NavigateToAsync<TViewModel>() where TViewModel : class =>
        NavigateToAsync(typeof(TViewModel));

    public async Task<bool> NavigateToAsync(Type viewModelType)
    {
        if (CurrentViewModelType == viewModelType)
        {
            return true;
        }

        if (CurrentViewModel is INavigationGuard guard)
        {
            if (!await guard.CanNavigateAwayAsync())
            {
                return false;
            }
        }

        var next = _serviceProvider.GetService(viewModelType);
        if (next is null)
        {
            MessageBox.Show(
                "The requested page could not be loaded.",
                "Navigation",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }

        CurrentViewModel = next;
        CurrentViewModelType = viewModelType;
        CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }
}
