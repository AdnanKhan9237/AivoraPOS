using CommunityToolkit.Mvvm.ComponentModel;

namespace NovaPOS.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusMessage = "Ready";
}
