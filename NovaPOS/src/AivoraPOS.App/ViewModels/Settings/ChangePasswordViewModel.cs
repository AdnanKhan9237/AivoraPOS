using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AivoraPOS.App.ViewModels.Settings;

public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly Action<(string Current, string New)?> _close;

    public ChangePasswordViewModel(Action<(string Current, string New)?> close)
    {
        _close = close;
    }

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string? _validationMessage;

    [RelayCommand]
    private void Save()
    {
        ValidationMessage = null;
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            ValidationMessage = "All fields are required.";
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ValidationMessage = "New passwords do not match.";
            return;
        }

        _close((CurrentPassword, NewPassword));
    }

    [RelayCommand]
    private void Cancel() => _close(null);
}
