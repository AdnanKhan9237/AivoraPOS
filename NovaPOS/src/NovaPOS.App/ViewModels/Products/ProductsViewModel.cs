using CommunityToolkit.Mvvm.ComponentModel;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.App.ViewModels.Products;

[RequiresPermission(Permission.ManageProducts)]
public partial class ProductsViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IInventoryService _inventoryService;
    private readonly IProductImportService _productImportService;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly ICurrentUserService _currentUserService;

    public ProductsViewModel(
        IProductService productService,
        ICategoryService categoryService,
        IInventoryService inventoryService,
        IProductImportService productImportService,
        IInventoryAlertService inventoryAlertService,
        ICurrentUserService currentUserService,
        Core.Interfaces.Licensing.ILicenseService licenseService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _inventoryService = inventoryService;
        _productImportService = productImportService;
        _inventoryAlertService = inventoryAlertService;
        _currentUserService = currentUserService;

        ProductList = new ProductListViewModel(
            productService,
            categoryService,
            licenseService,
            inventoryAlertService,
            currentUserService,
            EditProductAsync,
            AdjustStockAsync,
            ViewHistoryAsync,
            ImportCsvAsync,
            ManageCategoriesAsync);

        LowStock = new LowStockViewModel(inventoryAlertService, RestockProductAsync);

        _inventoryAlertService.LowStockCountChanged += (_, _) => UpdateLowStockBadge();
        UpdateLowStockBadge();
    }

    public ProductListViewModel ProductList { get; }
    public LowStockViewModel LowStock { get; }

    [ObservableProperty]
    private int _selectedTabIndex;

    [ObservableProperty]
    private string _lowStockBadgeText = string.Empty;

    [ObservableProperty]
    private bool _showLowStockBadge;

    public async Task RefreshLowStockBadgeAsync()
    {
        await _inventoryAlertService.RefreshAsync();
        UpdateLowStockBadge();
        await LowStock.LoadCommand.ExecuteAsync(null);
    }

    private void UpdateLowStockBadge()
    {
        ShowLowStockBadge = _inventoryAlertService.LowStockCount > 0;
        LowStockBadgeText = ShowLowStockBadge ? _inventoryAlertService.LowStockCount.ToString() : string.Empty;
    }

    private async Task EditProductAsync(ProductVm? product)
    {
        var tcs = new TaskCompletionSource<bool>();
        var vm = new ProductEditViewModel(
            product,
            _productService,
            _categoryService,
            _currentUserService,
            saved => tcs.TrySetResult(saved),
            ManageCategoriesAsync);

        var window = new Views.Products.ProductEditWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        if (await tcs.Task)
        {
            await ProductList.RefreshAfterChildDialogAsync();
            await RefreshLowStockBadgeAsync();
        }
    }

    private async Task AdjustStockAsync(ProductVm product)
    {
        var tcs = new TaskCompletionSource<bool>();
        var vm = new StockAdjustmentViewModel(
            product,
            _inventoryService,
            _currentUserService,
            saved => tcs.TrySetResult(saved));

        var window = new Views.Products.StockAdjustmentWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        if (await tcs.Task)
        {
            await ProductList.RefreshAfterChildDialogAsync();
            await RefreshLowStockBadgeAsync();
        }
    }

    private async Task RestockProductAsync(ProductVm product)
    {
        var tcs = new TaskCompletionSource<bool>();
        var vm = new StockAdjustmentViewModel(
            product,
            _inventoryService,
            _currentUserService,
            saved => tcs.TrySetResult(saved))
        {
            SelectedReason = StockAdjustmentReason.Restock
        };

        var window = new Views.Products.StockAdjustmentWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        if (await tcs.Task)
        {
            await ProductList.RefreshAfterChildDialogAsync();
            await RefreshLowStockBadgeAsync();
        }
    }

    private Task ViewHistoryAsync(ProductVm product)
    {
        var vm = new InventoryHistoryViewModel(
            product,
            _inventoryService,
            () => { });

        var window = new Views.Products.InventoryHistoryWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        return Task.CompletedTask;
    }

    private async Task ImportCsvAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var vm = new ProductImportViewModel(
            _productImportService,
            _currentUserService,
            imported => tcs.TrySetResult(imported));

        var window = new Views.Products.ProductImportWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        if (await tcs.Task)
        {
            await ProductList.RefreshAfterChildDialogAsync();
            await RefreshLowStockBadgeAsync();
        }
    }

    private async Task ManageCategoriesAsync()
    {
        var tcs = new TaskCompletionSource();
        var vm = new CategoryManagementViewModel(
            _categoryService,
            () => tcs.TrySetResult());

        var window = new Views.Products.CategoryManagementWindow
        {
            DataContext = vm,
            Owner = System.Windows.Application.Current.MainWindow
        };

        window.ShowDialog();
        await tcs.Task;
        await ProductList.RefreshAfterChildDialogAsync();
    }
}
