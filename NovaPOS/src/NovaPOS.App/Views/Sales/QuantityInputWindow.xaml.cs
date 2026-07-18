using System.Windows;
using System.Windows.Input;

namespace NovaPOS.App.Views.Sales;

public partial class QuantityInputWindow
{
    public QuantityInputWindow(int currentQuantity)
    {
        InitializeComponent();
        QuantityBox.Text = currentQuantity.ToString();
        QuantityBox.SelectAll();
        Loaded += (_, _) => QuantityBox.Focus();
    }

    public int? EnteredQuantity { get; private set; }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(QuantityBox.Text, out var qty) && qty > 0)
        {
            EnteredQuantity = qty;
            DialogResult = true;
            Close();
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
        }
        else if (e.Key == Key.Enter)
        {
            OnOk(this, new RoutedEventArgs());
        }
    }
}
