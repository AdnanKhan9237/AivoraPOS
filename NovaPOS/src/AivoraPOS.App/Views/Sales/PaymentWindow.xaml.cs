using System.Windows;
using System.Windows.Input;
using AivoraPOS.App.ViewModels.Sales;

namespace AivoraPOS.App.Views.Sales;

public partial class PaymentWindow
{
    public PaymentWindow()
    {
        InitializeComponent();
    }

    private void OnConfirm(object sender, RoutedEventArgs e)
    {
        if (DataContext is PaymentViewModel vm)
        {
            vm.ConfirmCommand.Execute(null);
            if (vm.IsConfirmed)
            {
                DialogResult = true;
                Close();
            }
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            OnCancel(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }

        if (DataContext is not PaymentViewModel vm)
        {
            return;
        }

        if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
        {
            vm.AppendDigitCommand.Execute(((int)e.Key - (int)Key.NumPad0).ToString());
            e.Handled = true;
        }
    }
}
