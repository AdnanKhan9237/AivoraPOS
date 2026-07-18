using System.Windows;
using System.Windows.Controls;

namespace AivoraPOS.App.Views.Shell;

public partial class LockOverlayView
{
    public LockOverlayView()
    {
        InitializeComponent();
    }

    private void OnPinPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Shell.LockOverlayViewModel vm && sender is PasswordBox box)
        {
            vm.Pin = box.Password;
        }
    }

    private void OnManagerPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Shell.LockOverlayViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }
}
