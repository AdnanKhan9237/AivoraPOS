using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NovaPOS.App.ViewModels.Shell;

public partial class NavigationItemViewModel : ObservableObject
{
    public NavigationItemViewModel(
        string key,
        string label,
        string icon,
        Type viewModelType,
        Func<Task> navigateAsync)
    {
        Key = key;
        Label = label;
        Icon = icon;
        ViewModelType = viewModelType;
        _navigateAsync = navigateAsync;
    }

    private readonly Func<Task> _navigateAsync;

    public string Key { get; }
    public string Label { get; }
    public string Icon { get; }
    public Type ViewModelType { get; }

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _showLockedIcon;

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string? _badgeText;

    [ObservableProperty]
    private bool _showBadge;

    [RelayCommand]
    private Task NavigateAsync() => _navigateAsync();
}
