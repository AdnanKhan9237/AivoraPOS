using System.Windows;
using System.Windows.Controls;

namespace NovaPOS.App.Views.Settings;

public partial class UserEditWindow
{
    public UserEditWindow()
    {
        InitializeComponent();
    }

    private void OnPinChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.UserEditViewModel vm && sender is PasswordBox box)
        {
            vm.Pin = box.Password;
        }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.UserEditViewModel vm && sender is PasswordBox box)
        {
            vm.Password = box.Password;
        }
    }
}
