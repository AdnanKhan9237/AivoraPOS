using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Models;

namespace AivoraPOS.App.ViewModels;

public partial class ManagerOverrideViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly Action<bool> _closeAction;

    public ManagerOverrideViewModel(IAuthService authService, Action<bool> closeAction)
    {
        _authService = authService;
        _closeAction = closeAction;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task ApproveAsync()
    {
        var result = await _authService.ManagerOverrideAsync(Username, Password);
        if (!result.Success)
        {
            StatusMessage = result.Message;
            return;
        }

        _closeAction(true);
    }

    [RelayCommand]
    private void Cancel() => _closeAction(false);
}
