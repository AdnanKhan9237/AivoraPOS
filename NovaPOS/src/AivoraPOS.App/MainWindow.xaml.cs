using System.Windows;
using System.Windows.Input;
using AivoraPOS.Core.Interfaces.Security;

namespace AivoraPOS.App;

public partial class MainWindow : Window
{
    private readonly ISessionTimeoutService _sessionTimeoutService;

    public MainWindow(ISessionTimeoutService sessionTimeoutService)
    {
        _sessionTimeoutService = sessionTimeoutService;
        InitializeComponent();
        PreviewMouseMove += OnUserActivity;
        PreviewKeyDown += OnUserActivity;
    }

    private void OnUserActivity(object sender, InputEventArgs e) => _sessionTimeoutService.RecordActivity();
}
