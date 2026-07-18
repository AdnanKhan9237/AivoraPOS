namespace AivoraPOS.Core.Models.Reports;

public enum InventoryStockStatus
{
    Healthy,
    LowStock,
    OutOfStock
}

public sealed class InventoryStatusDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int LowStockThreshold { get; init; }
    public InventoryStockStatus Status { get; init; }
}
