using System.Windows;
using System.Windows.Controls;

namespace NovaPOS.App.Views;

public partial class ManagerOverrideWindow
{
    public ManagerOverrideWindow()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ManagerOverrideViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }
}
