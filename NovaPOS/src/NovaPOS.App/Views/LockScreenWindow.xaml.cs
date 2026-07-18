using System.Windows;
using System.Windows.Controls;

namespace NovaPOS.App.Views;

public partial class LockScreenWindow
{
    public LockScreenWindow()
    {
        InitializeComponent();
    }

    private void OnPinPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LockScreenViewModel vm)
        {
            vm.Pin = PinPasswordBox.Password;
        }
    }

    private void OnManagerPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LockScreenViewModel vm)
        {
            vm.Password = ManagerPasswordBox.Password;
        }
    }
}
