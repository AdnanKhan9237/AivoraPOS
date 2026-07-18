using System.Windows.Controls;
using System.Windows.Input;

namespace NovaPOS.App.Views.Sales;

public partial class SalesScreen
{
    public SalesScreen()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        SearchBox.Focus();
        if (DataContext is ViewModels.Sales.SalesViewModel vm)
        {
            vm.OnFocusSearchRequested += () => SearchBox.Focus();
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is ViewModels.Sales.SalesViewModel { IsCheckoutOpen: true })
        {
            e.Handled = true;
        }
    }

    private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is ViewModels.Sales.SalesViewModel vm)
        {
            vm.OnSearchKeyDown();
        }
    }
}
