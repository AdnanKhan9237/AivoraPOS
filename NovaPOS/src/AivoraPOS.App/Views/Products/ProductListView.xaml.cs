using System.Windows.Controls;
using System.Windows.Input;

namespace AivoraPOS.App.Views.Products;

public partial class ProductListView
{
    public ProductListView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ViewModels.Products.ProductListViewModel vm && vm.SelectedProduct is not null)
        {
            _ = vm.EditProductAsync(vm.SelectedProduct);
        }
    }
}
