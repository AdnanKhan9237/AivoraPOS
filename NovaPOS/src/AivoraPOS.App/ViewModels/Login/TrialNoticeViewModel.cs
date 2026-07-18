using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AivoraPOS.App.ViewModels.Login;

public partial class TrialNoticeViewModel : ObservableObject
{
    public TrialNoticeViewModel(string message, Action onContinue)
    {
        Message = message;
        _onContinue = onContinue;
    }

    private readonly Action _onContinue;

    public string Message { get; }

    [RelayCommand]
    private void Continue()
    {
        _onContinue();
    }
}
