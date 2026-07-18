using System.Windows;
using System.Windows.Input;

namespace AivoraPOS.App.Views.Sales;

public partial class QuickSkuWindow
{
    public QuickSkuWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => SkuBox.Focus();
    }

    public string? EnteredSku { get; private set; }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        EnteredSku = SkuBox.Text;
        DialogResult = true;
        Close();
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
            OnAdd(this, new RoutedEventArgs());
        }
    }
}
