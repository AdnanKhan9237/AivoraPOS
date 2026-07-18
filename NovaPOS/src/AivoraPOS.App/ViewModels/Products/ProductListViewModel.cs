using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Attributes;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Licensing;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Products;
using AivoraPOS.Licensing.Extensions;

namespace AivoraPOS.App.ViewModels.Products;

[RequiresPermission(Permission.ManageProducts)]
public partial class ProductListViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILicenseService _licenseService;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Func<ProductVm?, Task> _editProduct;
    private readonly Func<ProductVm, Task> _adjustStock;
    private readonly Func<ProductVm, Task> _viewHistory;
    private readonly Func<Task> _importCsv;
    private readonly Func<Task> _manageCategories;
    private CancellationTokenSource? _searchDebounceCts;

    public ProductListViewModel(
        IProductService productService,
        ICategoryService categoryService,
        ILicenseService licenseService,
        IInventoryAlertService inventoryAlertService,
        ICurrentUserService currentUserService,
        Func<ProductVm?, Task> editProduct,
        Func<ProductVm, Task> adjustStock,
        Func<ProductVm, Task> viewHistory,
        Func<Task> importCsv,
        Func<Task> manageCategories)
    {
        _productService = productService;
        _categoryService = categoryService;
        _licenseService = licenseService;
        _inventoryAlertService = inventoryAlertService;
        _currentUserService = currentUserService;
        _editProduct = editProduct;
        _adjustStock = adjustStock;
        _viewHistory = viewHistory;
        _importCsv = importCsv;
        _manageCategories = manageCategories;

        StatusOptions =
        [
            ProductStatusFilter.All,
            ProductStatusFilter.Active,
            ProductStatusFilter.Inactive
        ];

        StockLevelOptions =
        [
            StockLevelFilter.All,
            StockLevelFilter.Low,
            StockLevelFilter.Adequate,
            StockLevelFilter.OutOfStock
        ];

        PageSize = 50;
        _ = InitializeAsync();
    }

    public ObservableCollection<ProductVm> Products { get; } = [];
    public ObservableCollection<ProductCategoryVm> Categories { get; } = [];

    public IReadOnlyList<ProductStatusFilter> StatusOptions { get; }
    public IReadOnlyList<StockLevelFilter> StockLevelOptions { get; }

    public bool CanImportCsv => _licenseService.CanUse(LicenseFeature.ExportPdfExcel);

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private ProductCategoryVm? _filterCategory;

    [ObservableProperty]
    private ProductStatusFilter _filterStatus = ProductStatusFilter.All;

    [ObservableProperty]
    private StockLevelFilter _filterStockLevel = StockLevelFilter.All;

    [ObservableProperty]
    private ProductVm? _selectedProduct;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isLoading;

    public int PageSize { get; }

    partial void OnSearchTextChanged(string value) => _ = DebouncedReloadAsync();

    partial void OnFilterCategoryChanged(ProductCategoryVm? value) => _ = ReloadAsync();

    partial void OnFilterStatusChanged(ProductStatusFilter value) => _ = ReloadAsync();

    partial void OnFilterStockLevelChanged(StockLevelFilter value) => _ = ReloadAsync();

    [RelayCommand]
    private async Task ReloadAsync()
    {
        IsLoading = true;
        try
        {
            var query = new ProductListQuery
            {
                SearchTerm = SearchText,
                CategoryId = FilterCategory?.Id == Guid.Empty ? null : FilterCategory?.Id,
                Status = FilterStatus,
                StockLevel = FilterStockLevel,
                Page = CurrentPage,
                PageSize = PageSize
            };

            var result = await _productService.GetProductsPagedAsync(query);
            Products.Clear();
            foreach (var product in result.Items)
            {
                Products.Add(new ProductVm(product));
            }

            TotalCount = result.TotalCount;
            TotalPages = Math.Max(1, result.TotalPages);
            if (CurrentPage > TotalPages)
            {
                CurrentPage = TotalPages;
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await ReloadAsync();
        }
    }

    [RelayCommand]
    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await ReloadAsync();
    }

    [RelayCommand]
    private Task AddAsync() => _editProduct(null);

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private Task EditAsync() => SelectedProduct is null ? Task.CompletedTask : _editProduct(SelectedProduct);

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private async Task DeleteAsync()
    {
        if (SelectedProduct is null)
        {
            return;
        }

        var confirm = MessageBox.Show(
            $"Deactivate '{SelectedProduct.Name}'? The product will be hidden from sales but kept in history.",
            "Deactivate Product",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        var userId = _currentUserService.CurrentUser?.Id ?? Guid.Empty;
        await _productService.SoftDeleteAsync(SelectedProduct.Id, userId);
        await _inventoryAlertService.RefreshAsync();
        await ReloadAsync();
    }

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private Task AdjustStockAsync() => SelectedProduct is null ? Task.CompletedTask : _adjustStock(SelectedProduct);

    [RelayCommand(CanExecute = nameof(CanEditSelected))]
    private Task ViewHistoryAsync() => SelectedProduct is null ? Task.CompletedTask : _viewHistory(SelectedProduct);

    [RelayCommand]
    private async Task ImportCsvAsync()
    {
        try
        {
            _licenseService.RequireFeature(LicenseFeature.ExportPdfExcel);
            await _importCsv();
            await ReloadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Import Products", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private Task ManageCategoriesAsync() => _manageCategories();

    public Task EditProductAsync(ProductVm product) => _editProduct(product);

    public Task AdjustStockForProductAsync(ProductVm product) => _adjustStock(product);

    public Task ViewHistoryForProductAsync(ProductVm product) => _viewHistory(product);

    public async Task RefreshAfterChildDialogAsync()
    {
        await LoadCategoriesAsync();
        await ReloadAsync();
        await _inventoryAlertService.RefreshAsync();
    }

    private bool CanEditSelected() => SelectedProduct is not null;

    partial void OnSelectedProductChanged(ProductVm? value)
    {
        EditCommand.NotifyCanExecuteChanged();
        DeleteCommand.NotifyCanExecuteChanged();
        AdjustStockCommand.NotifyCanExecuteChanged();
        ViewHistoryCommand.NotifyCanExecuteChanged();
    }

    private async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        Categories.Insert(0, new ProductCategoryVm(Guid.Empty, "All categories"));
        FilterCategory = Categories[0];
        await ReloadAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        var allOption = Categories.FirstOrDefault(x => x.Id == Guid.Empty);
        Categories.Clear();
        if (allOption is not null)
        {
            Categories.Add(allOption);
        }

        foreach (var category in categories.Where(x => x.IsActive))
        {
            Categories.Add(new ProductCategoryVm(category));
        }
    }

    private async Task DebouncedReloadAsync()
    {
        _searchDebounceCts?.Cancel();
        _searchDebounceCts = new CancellationTokenSource();
        var token = _searchDebounceCts.Token;

        try
        {
            await Task.Delay(300, token);
            CurrentPage = 1;
            await ReloadAsync();
        }
        catch (TaskCanceledException)
        {
        }
    }
}
