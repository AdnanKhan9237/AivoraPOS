using System.Windows;
using System.Windows.Input;
using NovaPOS.App.ViewModels.Sales;

namespace NovaPOS.App.Views.Sales;

public partial class DiscountWindow
{
    public DiscountViewModel ViewModel { get; }

    public DiscountWindow(decimal orderDiscount, IReadOnlyList<CartItemVm> cartItems)
    {
        InitializeComponent();
        ViewModel = new DiscountViewModel(orderDiscount, cartItems);
        DataContext = ViewModel;
    }

    private void OnApply(object sender, RoutedEventArgs e)
    {
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
    }
}
