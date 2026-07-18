using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Products;

namespace AivoraPOS.App.ViewModels.Products;

public partial class ProductEditViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action<bool> _close;
    private readonly Func<Task>? _manageCategories;
    private readonly Guid? _productId;
    private decimal _originalSalePrice;

    public ProductEditViewModel(
        ProductVm? product,
        IProductService productService,
        ICategoryService categoryService,
        ICurrentUserService currentUserService,
        Action<bool> close,
        Func<Task>? manageCategories = null)
    {
        _productService = productService;
        _categoryService = categoryService;
        _currentUserService = currentUserService;
        _close = close;
        _manageCategories = manageCategories;
        _productId = product?.Id;
        IsEditMode = product is not null;

        if (product is not null)
        {
            Name = product.Name;
            Sku = product.Sku;
            Barcode = product.Barcode;
            PurchasePrice = product.PurchasePrice;
            SalePrice = product.SalePrice;
            _originalSalePrice = product.SalePrice;
            TaxRatePercent = product.TaxRate * 100m;
            StockQuantity = product.StockQuantity;
            LowStockThreshold = product.LowStockThreshold;
            IsActive = product.IsActive;
            SelectedCategoryId = product.CategoryId;
        }

        _ = InitializeAsync();
    }

    public bool IsEditMode { get; }

    public ObservableCollection<ProductCategoryVm> Categories { get; } = [];

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string? _barcode;

    [ObservableProperty]
    private Guid _selectedCategoryId;

    [ObservableProperty]
    private decimal _purchasePrice;

    [ObservableProperty]
    private decimal _salePrice;

    [ObservableProperty]
    private decimal _taxRatePercent = 8.25m;

    [ObservableProperty]
    private int _stockQuantity;

    [ObservableProperty]
    private int _lowStockThreshold = 5;

    [ObservableProperty]
    private bool _isActive = true;

    [ObservableProperty]
    private string? _validationMessage;

    [ObservableProperty]
    private string? _priceWarning;

    [ObservableProperty]
    private bool _isSaving;

    partial void OnSalePriceChanged(decimal value)
    {
        PriceWarning = value > 0 && PurchasePrice > 0 && value <= PurchasePrice
            ? "Sale price should be greater than purchase price."
            : null;
    }

    partial void OnPurchasePriceChanged(decimal value)
    {
        PriceWarning = SalePrice > 0 && value > 0 && SalePrice <= value
            ? "Sale price should be greater than purchase price."
            : null;
    }

    [RelayCommand]
    private async Task AddCategoryAsync()
    {
        if (_manageCategories is null)
        {
            return;
        }

        await _manageCategories();
        await InitializeAsync();
    }

    [RelayCommand]
    private async Task GenerateSkuAsync()
    {
        Sku = await _productService.GenerateSkuAsync();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidationMessage = null;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Product name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Sku))
        {
            ValidationMessage = "SKU is required.";
            return;
        }

        if (SelectedCategoryId == Guid.Empty)
        {
            ValidationMessage = "Category is required.";
            return;
        }

        if (PurchasePrice < 0 || SalePrice < 0)
        {
            ValidationMessage = "Prices must be positive values.";
            return;
        }

        if (!await _productService.IsSkuUniqueAsync(Sku, _productId))
        {
            ValidationMessage = $"SKU '{Sku}' is already in use.";
            return;
        }

        if (IsEditMode && SalePrice != _originalSalePrice)
        {
            var confirm = MessageBox.Show(
                "Changing the sale price affects all future sales. Existing unfinished sales keep their snapshotted prices.\n\nContinue?",
                "Confirm Price Change",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
            {
                return;
            }
        }

        var userId = _currentUserService.CurrentUser?.Id ?? Guid.Empty;
        IsSaving = true;

        try
        {
            await _productService.SaveAsync(new ProductSaveRequest
            {
                Id = _productId,
                Name = Name,
                Sku = Sku,
                Barcode = Barcode,
                CategoryId = SelectedCategoryId,
                PurchasePrice = PurchasePrice,
                SalePrice = SalePrice,
                TaxRate = TaxRatePercent / 100m,
                StockQuantity = StockQuantity,
                LowStockThreshold = LowStockThreshold,
                IsActive = IsActive,
                UserId = userId
            });

            _close(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = ex.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _close(false);

    private async Task InitializeAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        Categories.Clear();
        foreach (var category in categories.Where(x => x.IsActive))
        {
            Categories.Add(new ProductCategoryVm(category));
        }

        if (!IsEditMode)
        {
            TaxRatePercent = (await _productService.GetDefaultTaxRateAsync()) * 100m;
            if (Categories.Count > 0)
            {
                SelectedCategoryId = Categories[0].Id;
            }
        }
        else if (SelectedCategoryId != Guid.Empty && Categories.All(x => x.Id != SelectedCategoryId))
        {
            var product = await _productService.GetByIdAsync(_productId!.Value);
            if (product?.Category is not null)
            {
                Categories.Add(new ProductCategoryVm(product.Category));
            }
        }
    }
}
