using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;

namespace AivoraPOS.App.ViewModels.Shell;

public partial class LockOverlayViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public LockOverlayViewModel(
        IAuthService authService,
        IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
        _ = LoadUsersAsync();
    }

    public event Action? Unlocked;

    public ObservableCollection<User> Cashiers { get; } = [];

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

    [ObservableProperty]
    private string _lockedAtText = string.Empty;

    public void SetLockedAt(DateTime lockedAtLocal)
    {
        LockedAtText = $"Locked at {lockedAtLocal:g}";
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        StatusMessage = string.Empty;
        Core.Models.AuthResult result;

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

        Pin = string.Empty;
        Password = string.Empty;
        StatusMessage = string.Empty;
        Unlocked?.Invoke();
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
