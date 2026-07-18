using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Models;

namespace AivoraPOS.App.ViewModels;

public partial class LockScreenViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly Action<User> _onLoginSuccess;

    public LockScreenViewModel(
        IAuthService authService,
        IUserRepository userRepository,
        Action<User> onLoginSuccess)
    {
        _authService = authService;
        _userRepository = userRepository;
        _onLoginSuccess = onLoginSuccess;
        _ = LoadUsersAsync();
    }

    public ObservableCollection<User> Cashiers { get; } = new();

    [ObservableProperty]
    private User? _selectedCashier;

    [ObservableProperty]
    private string _pin = string.Empty;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isManagerLogin;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [RelayCommand]
    private async Task LoginAsync()
    {
        StatusMessage = string.Empty;
        AuthResult result;

        if (IsManagerLogin)
        {
            result = await _authService.LoginWithPasswordAsync(Username, Password);
        }
        else
        {
            if (SelectedCashier is null)
            {
                StatusMessage = "Select your name to continue.";
                return;
            }

            result = await _authService.LoginWithPinAsync(SelectedCashier.Id, Pin);
        }

        if (!result.Success || result.User is null)
        {
            StatusMessage = result.Message;
            return;
        }

        _onLoginSuccess(result.User);
    }

    private async Task LoadUsersAsync()
    {
        var users = await _userRepository.GetActiveCashiersAsync();
        Cashiers.Clear();
        foreach (var user in users)
        {
            Cashiers.Add(user);
        }

        SelectedCashier = Cashiers.FirstOrDefault();
    }
}
