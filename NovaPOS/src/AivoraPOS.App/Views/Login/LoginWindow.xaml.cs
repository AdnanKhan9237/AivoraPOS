using System.Windows;
using System.Windows.Controls;

namespace AivoraPOS.App.Views.Login;

public partial class LoginWindow
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    private void OnPinPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LockScreenViewModel vm && sender is PasswordBox box)
        {
            vm.Pin = box.Password;
        }
    }

    private void OnManagerPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LockScreenViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }
}
