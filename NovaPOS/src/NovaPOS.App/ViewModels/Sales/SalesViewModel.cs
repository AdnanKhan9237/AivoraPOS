using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NovaPOS.App.Views.Sales;
using NovaPOS.Core.Attributes;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Sales;

namespace NovaPOS.App.ViewModels.Sales;

[RequiresPermission(Permission.ProcessSale)]
public partial class SalesViewModel : ObservableObject, IDisposable
{
    private static readonly Guid AllCategoriesId = Guid.Empty;

    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISaleService _saleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IAuthService _authService;
    private readonly ILicenseService _licenseService;
    private readonly IReceiptService _receiptService;
    private readonly DispatcherTimer _searchDebounceTimer;
    private readonly DispatcherTimer _barcodeTimer;

    private List<Product> _catalog = new();
    private string _pendingBarcodeBuffer = string.Empty;
    private DateTime _lastSearchKeyUtc = DateTime.MinValue;
    private bool _isBarcodeScan;

    public SalesViewModel(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISaleService saleService,
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        IAuthService authService,
        ILicenseService licenseService,
        IReceiptService receiptService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _saleService = saleService;
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _authService = authService;
        _licenseService = licenseService;
        _receiptService = receiptService;

        _searchDebounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _searchDebounceTimer.Tick += async (_, _) =>
        {
            _searchDebounceTimer.Stop();
            await SearchAsync();
        };

        _barcodeTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _barcodeTimer.Tick += async (_, _) =>
        {
            _barcodeTimer.Stop();
            if (_pendingBarcodeBuffer.Length > 5)
            {
                _isBarcodeScan = true;
                SearchQuery = _pendingBarcodeBuffer;
                _pendingBarcodeBuffer = string.Empty;
                await SearchAsync();
                await TryAutoAddBarcodeAsync(SearchQuery);
                _isBarcodeScan = false;
            }
        };

        _ = InitializeAsync();
    }

    public ObservableCollection<CartItemVm> CartItems { get; } = new();
    public ObservableCollection<ProductTileVm> DisplayedProducts { get; } = new();
    public ObservableCollection<CategoryVm> Categories { get; } = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private decimal _subTotal;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    private decimal _discountAmount;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private CategoryVm? _selectedCategory;

    [ObservableProperty]
    private bool _isCheckoutOpen;

    [ObservableProperty]
    private bool _showOutOfStock;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage = "Loading products...";

    public bool CanApplyDiscount => _authorizationService.HasPermission(Permission.ApplyDiscount);
    public bool HasCartItems => CartItems.Count > 0;

    partial void OnSearchQueryChanged(string value)
    {
        if (_isBarcodeScan)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if ((now - _lastSearchKeyUtc).TotalMilliseconds < 30 && value.Length > _pendingBarcodeBuffer.Length)
        {
            _pendingBarcodeBuffer = value;
            _barcodeTimer.Stop();
            _barcodeTimer.Start();
        }
        else
        {
            _pendingBarcodeBuffer = string.Empty;
        }

        _lastSearchKeyUtc = now;
        _searchDebounceTimer.Stop();
        _searchDebounceTimer.Start();
    }

    partial void OnSelectedCategoryChanged(CategoryVm? value) => _ = SearchAsync();

    partial void OnShowOutOfStockChanged(bool value) => _ = SearchAsync();

    public void OnSearchKeyDown()
    {
        _lastSearchKeyUtc = DateTime.UtcNow;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var query = SearchQuery.Trim();
        IEnumerable<Product> source = _catalog;

        if (!ShowOutOfStock)
        {
            source = source.Where(p => p.StockQuantity > 0);
        }

        if (SelectedCategory is not null && SelectedCategory.Id != AllCategoriesId)
        {
            source = source.Where(p => p.CategoryId == SelectedCategory.Id);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            source = source.Where(p =>
                p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sku, query, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Barcode, query, StringComparison.OrdinalIgnoreCase));
        }

        var results = source.Take(500).ToList();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            DisplayedProducts.Clear();
            foreach (var product in results)
            {
                DisplayedProducts.Add(new ProductTileVm(product));
            }
        });
    }

    [RelayCommand]
    private void AddToCart(ProductTileVm? product)
    {
        if (product is null || product.IsOutOfStock)
        {
            return;
        }

        var catalogProduct = _catalog.FirstOrDefault(p => p.Id == product.Id);
        if (catalogProduct is null)
        {
            return;
        }

        var existing = CartItems.FirstOrDefault(x => x.ProductId == product.Id);
        if (existing is not null)
        {
            if (existing.Quantity >= catalogProduct.StockQuantity)
            {
                StatusMessage = $"Only {catalogProduct.StockQuantity} in stock for {product.Name}.";
                return;
            }

            existing.Quantity++;
        }
        else
        {
            var item = new CartItemVm(product) { TaxRate = catalogProduct.TaxRate };
            item.TotalsChanged += RecalculateTotals;
            CartItems.Add(item);
        }

        RecalculateTotals();
        OnPropertyChanged(nameof(HasCartItems));
        StatusMessage = $"Added {product.Name} to cart.";
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemVm? item)
    {
        if (item is null)
        {
            return;
        }

        item.TotalsChanged -= RecalculateTotals;
        CartItems.Remove(item);
        RecalculateTotals();
        OnPropertyChanged(nameof(HasCartItems));
    }

    [RelayCommand]
    private void UpdateQuantity((CartItemVm Item, int Delta) args)
    {
        var (item, delta) = args;
        var product = _catalog.FirstOrDefault(p => p.Id == item.ProductId);
        var newQty = item.Quantity + delta;
        if (newQty <= 0)
        {
            RemoveFromCart(item);
            return;
        }

        if (product is not null && newQty > product.StockQuantity)
        {
            StatusMessage = $"Only {product.StockQuantity} in stock.";
            return;
        }

        item.Quantity = newQty;
        RecalculateTotals();
    }

    [RelayCommand]
    private void PromptQuantity(CartItemVm? item)
    {
        if (item is null)
        {
            return;
        }

        var window = new QuantityInputWindow(item.Quantity);
        if (window.ShowDialog() == true && window.EnteredQuantity is int qty)
        {
            SetQuantity(item, qty);
        }
    }

    [RelayCommand]
    private async Task ApplyDiscountAsync()
    {
        if (!CanApplyDiscount)
        {
            if (!await RequestManagerOverrideAsync())
            {
                return;
            }
        }

        var window = new DiscountWindow(DiscountAmount, CartItems.ToList());
        if (window.ShowDialog() == true && window.ViewModel is DiscountViewModel vm)
        {
            DiscountAmount = vm.OrderDiscountAmount;
            foreach (var lineVm in vm.LineDiscounts)
            {
                var cartItem = CartItems.FirstOrDefault(x => x.ProductId == lineVm.ProductId);
                if (cartItem is not null)
                {
                    cartItem.LineDiscount = lineVm.DiscountAmount;
                }
            }

            RecalculateTotals();
        }
    }

    [RelayCommand]
    private void ClearCart()
    {
        foreach (var item in CartItems)
        {
            item.TotalsChanged -= RecalculateTotals;
        }

        CartItems.Clear();
        DiscountAmount = 0;
        RecalculateTotals();
        OnPropertyChanged(nameof(HasCartItems));
        StatusMessage = "Cart cleared.";
    }

    [RelayCommand]
    private void Checkout()
    {
        if (!HasCartItems)
        {
            StatusMessage = "Cart is empty.";
            return;
        }

        IsCheckoutOpen = true;
        var paymentVm = new PaymentViewModel(TotalAmount);
        var window = new PaymentWindow { DataContext = paymentVm, Owner = Application.Current.MainWindow };
        IsCheckoutOpen = true;

        if (window.ShowDialog() == true && paymentVm.IsConfirmed)
        {
            _ = CompleteSaleAsync(paymentVm);
        }

        IsCheckoutOpen = false;
    }

    [RelayCommand]
    private async Task QuickAddBySkuAsync()
    {
        var window = new QuickSkuWindow();
        if (window.ShowDialog() != true || string.IsNullOrWhiteSpace(window.EnteredSku))
        {
            return;
        }

        var product = _catalog.FirstOrDefault(p =>
            string.Equals(p.Sku, window.EnteredSku.Trim(), StringComparison.OrdinalIgnoreCase));

        if (product is null)
        {
            product = await _productRepository.GetBySkuAsync(window.EnteredSku.Trim());
            if (product is not null && !_catalog.Any(p => p.Id == product.Id))
            {
                _catalog.Add(product);
            }
        }

        if (product is null)
        {
            StatusMessage = "SKU not found.";
            return;
        }

        AddToCart(new ProductTileVm(product));
    }

    [RelayCommand]
    private void FocusSearch() => OnFocusSearchRequested?.Invoke();

    [RelayCommand]
    private void IncreaseQuantity(CartItemVm? item)
    {
        if (item is not null)
        {
            UpdateQuantity((item, 1));
        }
    }

    [RelayCommand]
    private void DecreaseQuantity(CartItemVm? item)
    {
        if (item is not null)
        {
            UpdateQuantity((item, -1));
        }
    }

    public void FocusSearchRequested() => OnFocusSearchRequested?.Invoke();
    public event Action? OnFocusSearchRequested;

    private async Task InitializeAsync()
    {
        IsBusy = true;
        try
        {
            var categoriesTask = _categoryRepository.GetActiveAsync();
            var catalogTask = Task.Run(async () => await _productRepository.GetActiveCatalogAsync());
            await Task.WhenAll(categoriesTask, catalogTask);

            _catalog = catalogTask.Result.ToList();

            Categories.Clear();
            Categories.Add(new CategoryVm(AllCategoriesId, "All"));
            foreach (var category in categoriesTask.Result)
            {
                Categories.Add(new CategoryVm(category));
            }

            SelectedCategory = Categories.FirstOrDefault();
            await SearchAsync();
            StatusMessage = $"{_catalog.Count} products loaded.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task TryAutoAddBarcodeAsync(string barcode)
    {
        var product = _catalog.FirstOrDefault(p => string.Equals(p.Barcode, barcode, StringComparison.OrdinalIgnoreCase))
            ?? await _productRepository.GetByBarcodeAsync(barcode);

        if (product is null)
        {
            StatusMessage = "Barcode not found.";
            return;
        }

        if (!_catalog.Any(p => p.Id == product.Id))
        {
            _catalog.Add(product);
        }

        AddToCart(new ProductTileVm(product));
        SearchQuery = string.Empty;
        await SearchAsync();
    }

    private void SetQuantity(CartItemVm item, int quantity)
    {
        var product = _catalog.FirstOrDefault(p => p.Id == item.ProductId);
        if (quantity <= 0)
        {
            RemoveFromCart(item);
            return;
        }

        if (product is not null && quantity > product.StockQuantity)
        {
            StatusMessage = $"Only {product.StockQuantity} in stock.";
            return;
        }

        item.Quantity = quantity;
        RecalculateTotals();
    }

    private void RecalculateTotals()
    {
        SubTotal = CartItems.Sum(x => x.NetAmount);
        TaxAmount = CartItems.Sum(x => x.TaxAmount);
        TotalAmount = Math.Max(0, SubTotal + TaxAmount - DiscountAmount);
        OnPropertyChanged(nameof(HasCartItems));
    }

    private async Task CompleteSaleAsync(PaymentViewModel payment)
    {
        var cashier = _currentUserService.CurrentUser;
        if (cashier is null)
        {
            MessageBox.Show("You must be signed in to complete a sale.", "NovaPOS", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var request = new CompleteSaleRequest
            {
                CashierId = cashier.Id,
                Lines = CartItems.Select(x => new CartLine
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ProductSku = x.ProductSku,
                    UnitPrice = x.UnitPrice,
                    TaxRate = x.TaxRate,
                    Quantity = x.Quantity,
                    LineDiscount = x.LineDiscount
                }).ToList(),
                OrderDiscountAmount = DiscountAmount,
                PaymentMethod = payment.PaymentMethod,
                AmountPaid = payment.AmountPaid,
                Change = payment.Change,
                SubTotal = SubTotal,
                TaxAmount = TaxAmount,
                TotalAmount = TotalAmount,
                ShowReceiptWatermark = _licenseService.ShowReceiptWatermark
            };

            var result = await _saleService.CompleteSaleAsync(request);
            ShowSaleCompleteDialog(result);
            ClearCart();
            await RefreshCatalogAsync();
            StatusMessage = $"Sale {result.Sale.SaleNumber} completed.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Sale Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Sale could not be completed.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ShowSaleCompleteDialog(CompletedSaleResult result)
    {
        var canEmail = _licenseService.EffectivePlan is LicensePlan.Professional or LicensePlan.Enterprise;
        var vm = new SaleCompleteViewModel(result, canEmail, _receiptService);
        var window = new SaleCompleteWindow { DataContext = vm, Owner = Application.Current.MainWindow };
        window.ShowDialog();
    }

    private async Task RefreshCatalogAsync()
    {
        _catalog = (await _productRepository.GetActiveCatalogAsync()).ToList();
        await SearchAsync();
    }

    private async Task<bool> RequestManagerOverrideAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var window = new Views.ManagerOverrideWindow();
        window.DataContext = new ManagerOverrideViewModel(_authService, success =>
        {
            tcs.TrySetResult(success);
            window.Close();
        });
        window.Owner = Application.Current.MainWindow;
        window.ShowDialog();
        return await tcs.Task;
    }

    public void Dispose()
    {
        _searchDebounceTimer.Stop();
        _barcodeTimer.Stop();
    }
}
