using CommunityToolkit.Mvvm.ComponentModel;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.App.ViewModels.Sales;

public partial class ProductTileVm : ObservableObject
{
    public ProductTileVm(Product product)
    {
        Id = product.Id;
        Name = product.Name;
        Sku = product.Sku;
        Barcode = product.Barcode;
        SalePrice = product.SalePrice;
        StockQuantity = product.StockQuantity;
        LowStockThreshold = product.LowStockThreshold;
        CategoryId = product.CategoryId;
        IsActive = product.IsActive;
    }

    public Guid Id { get; }
    public string Name { get; }
    public string Sku { get; }
    public string? Barcode { get; }
    public decimal SalePrice { get; }
    public int StockQuantity { get; }
    public int LowStockThreshold { get; }
    public Guid CategoryId { get; }
    public bool IsActive { get; }

    public bool IsOutOfStock => StockQuantity <= 0;
    public bool IsLowStock => StockQuantity > 0 && StockQuantity <= LowStockThreshold;
    public string PriceDisplay => SalePrice.ToString("C");
    public string StockDisplay => IsOutOfStock ? "OUT" : StockQuantity.ToString();
}
