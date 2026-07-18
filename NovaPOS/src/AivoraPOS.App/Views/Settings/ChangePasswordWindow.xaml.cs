using System.Windows;
using System.Windows.Controls;

namespace AivoraPOS.App.Views.Settings;

public partial class ChangePasswordWindow
{
    public ChangePasswordWindow()
    {
        InitializeComponent();
    }

    private void OnCurrentChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.ChangePasswordViewModel vm && sender is PasswordBox box)
        {
            vm.CurrentPassword = box.Password;
        }
    }

    private void OnNewChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.ChangePasswordViewModel vm && sender is PasswordBox box)
        {
            vm.NewPassword = box.Password;
        }
    }

    private void OnConfirmChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Settings.ChangePasswordViewModel vm && sender is PasswordBox box)
        {
            vm.ConfirmPassword = box.Password;
        }
    }
}
