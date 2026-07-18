using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NovaPOS.App.ViewModels;

public partial class LicenseGateViewModel : ObservableObject
{
    private readonly Action<bool> _closeAction;

    public LicenseGateViewModel(string message, Action<bool> closeAction)
    {
        Message = message;
        _closeAction = closeAction;
    }

    public string Message { get; }

    [RelayCommand]
    private void Activate() => _closeAction(true);

    [RelayCommand]
    private void ContinueReadOnly() => _closeAction(false);

    [RelayCommand]
    private void Exit() => _closeAction(false);
}
