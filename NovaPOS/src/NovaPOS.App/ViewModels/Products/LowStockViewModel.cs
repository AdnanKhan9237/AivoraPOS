using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.ViewModels.Products;

[RequiresPermission(Permission.ManageProducts)]
public partial class LowStockViewModel : ObservableObject
{
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly Func<ProductVm, Task> _restockProduct;

    public LowStockViewModel(
        IInventoryAlertService inventoryAlertService,
        Func<ProductVm, Task> restockProduct)
    {
        _inventoryAlertService = inventoryAlertService;
        _restockProduct = restockProduct;
        _ = LoadAsync();
    }

    public ObservableCollection<ProductVm> Products { get; } = [];

    [ObservableProperty]
    private ProductVm? _selectedProduct;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            var products = await _inventoryAlertService.GetLowStockProductsAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(new ProductVm(product));
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(HasSelection))]
    private Task RestockAsync() => SelectedProduct is null ? Task.CompletedTask : _restockProduct(SelectedProduct);

    partial void OnSelectedProductChanged(ProductVm? value) => RestockCommand.NotifyCanExecuteChanged();

    private bool HasSelection() => SelectedProduct is not null;
}
