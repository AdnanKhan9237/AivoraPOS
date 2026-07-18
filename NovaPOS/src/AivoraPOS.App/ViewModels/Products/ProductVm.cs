using CommunityToolkit.Mvvm.ComponentModel;
using AivoraPOS.Core.Entities;

namespace AivoraPOS.App.ViewModels.Products;

public partial class ProductVm : ObservableObject
{
    public ProductVm(Product product)
    {
        Id = product.Id;
        Sku = product.Sku;
        Name = product.Name;
        Barcode = product.Barcode;
        CategoryId = product.CategoryId;
        CategoryName = product.Category?.Name ?? string.Empty;
        PurchasePrice = product.PurchasePrice;
        SalePrice = product.SalePrice;
        TaxRate = product.TaxRate;
        StockQuantity = product.StockQuantity;
        LowStockThreshold = product.LowStockThreshold;
        IsActive = product.IsActive;
    }

    public Guid Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string? Barcode { get; }
    public Guid CategoryId { get; }
    public string CategoryName { get; }
    public decimal PurchasePrice { get; }
    public decimal SalePrice { get; }
    public decimal TaxRate { get; }
    public int StockQuantity { get; }
    public int LowStockThreshold { get; }
    public bool IsActive { get; }

    public string StatusText => IsActive ? "Active" : "Inactive";

    public string StockLevelKey
    {
        get
        {
            if (StockQuantity <= LowStockThreshold)
            {
                return "Low";
            }

            if (StockQuantity <= LowStockThreshold * 2)
            {
                return "Warning";
            }

            return "Good";
        }
    }
}
