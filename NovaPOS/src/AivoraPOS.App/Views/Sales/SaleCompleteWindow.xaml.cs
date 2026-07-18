using System.Windows;

namespace AivoraPOS.App.Views.Sales;

public partial class SaleCompleteWindow
{
    public SaleCompleteWindow()
    {
        InitializeComponent();
    }

    private void OnDone(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.Sales.SaleCompleteViewModel vm)
        {
            vm.CloseCommand.Execute(null);
        }

        Close();
    }
}
