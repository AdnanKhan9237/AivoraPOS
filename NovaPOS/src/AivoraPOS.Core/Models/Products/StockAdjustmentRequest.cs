using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models.Products;

public sealed class StockAdjustmentRequest
{
    public Guid ProductId { get; init; }
    public StockAdjustmentReason Reason { get; init; }
    public int NewQuantity { get; init; }
    public string? Notes { get; init; }
    public Guid UserId { get; init; }
}
