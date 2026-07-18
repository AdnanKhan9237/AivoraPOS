using CommunityToolkit.Mvvm.ComponentModel;

namespace NovaPOS.App.ViewModels.Sales;

public partial class CartItemVm : ObservableObject
{
    public CartItemVm(ProductTileVm product, int quantity = 1)
    {
        ProductId = product.Id;
        ProductName = product.Name;
        ProductSku = product.Sku;
        UnitPrice = product.SalePrice;
        TaxRate = 0; // set by SalesViewModel from product catalog
        Quantity = quantity;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
    public string ProductSku { get; }
    public decimal UnitPrice { get; }

    [ObservableProperty]
    private decimal _taxRate;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _lineDiscount;

    public decimal NetAmount => UnitPrice * Quantity - LineDiscount;
    public decimal TaxAmount => Math.Round(NetAmount * TaxRate, 2, MidpointRounding.AwayFromZero);
    public decimal LineTotal => NetAmount + TaxAmount;
    public string LineTotalDisplay => LineTotal.ToString("C");

    partial void OnQuantityChanged(int value) => NotifyTotalsChanged();
    partial void OnLineDiscountChanged(decimal value) => NotifyTotalsChanged();
    partial void OnTaxRateChanged(decimal value) => NotifyTotalsChanged();

    public event Action? TotalsChanged;

    private void NotifyTotalsChanged()
    {
        OnPropertyChanged(nameof(NetAmount));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(LineTotal));
        OnPropertyChanged(nameof(LineTotalDisplay));
        TotalsChanged?.Invoke();
    }
}
