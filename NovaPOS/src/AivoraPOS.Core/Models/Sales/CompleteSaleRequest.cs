using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models.Sales;

public sealed class CompleteSaleRequest
{
    public Guid CashierId { get; init; }
    public IReadOnlyList<CartLine> Lines { get; init; } = Array.Empty<CartLine>();
    public decimal OrderDiscountAmount { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal Change { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public bool ShowReceiptWatermark { get; init; }
}
