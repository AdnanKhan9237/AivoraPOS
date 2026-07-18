using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Enums;

namespace AivoraPOS.App.ViewModels.Settings;

public partial class UserEditViewModel : ObservableObject
{
    private readonly Guid? _userId;
    private readonly Action<bool> _close;

    public UserEditViewModel(Guid? userId, string? fullName, UserRole? role, Action<bool> close)
    {
        _userId = userId;
        _close = close;
        IsEditMode = userId.HasValue;
        FullName = fullName ?? string.Empty;
        Username = string.Empty;
        SelectedRole = role ?? UserRole.Cashier;
        Pin = string.Empty;
        Password = string.Empty;
    }

    public bool IsEditMode { get; }
    public Guid? UserId => _userId;

    public IReadOnlyList<UserRole> Roles { get; } = [UserRole.Cashier, UserRole.Manager, UserRole.Admin];

    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private UserRole _selectedRole;

    [ObservableProperty]
    private string _pin = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string? _validationMessage;

    [RelayCommand]
    private void Save() => _close(true);

    [RelayCommand]
    private void Cancel() => _close(false);
}
