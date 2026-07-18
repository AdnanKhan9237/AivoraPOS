using System.Windows;
using System.Windows.Controls;

namespace AivoraPOS.App.Views.Settings;

public partial class ResetPinWindow
{
    public string? Pin { get; private set; }

    public ResetPinWindow(string prompt)
    {
        InitializeComponent();
        DataContext = new { Prompt = prompt };
    }

    private void OnPinChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
        {
            Pin = box.Password;
        }
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}
