namespace NovaPOS.Core.Models.Products;

public sealed class ProductSaveRequest
{
    public Guid? Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Barcode { get; init; }
    public Guid CategoryId { get; init; }
    public decimal PurchasePrice { get; init; }
    public decimal SalePrice { get; init; }
    public decimal TaxRate { get; init; }
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; } = 5;
    public bool IsActive { get; init; } = true;
    public Guid UserId { get; init; }
}
