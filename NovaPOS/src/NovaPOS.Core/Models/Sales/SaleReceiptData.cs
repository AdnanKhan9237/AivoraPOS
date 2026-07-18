using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models.Sales;

public sealed class SaleReceiptData
{
    public string StoreName { get; init; } = string.Empty;
    public string? FooterMessage { get; init; }
    public string SaleNumber { get; init; } = string.Empty;
    public DateTime SaleDateUtc { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public PaymentMethod PaymentMethod { get; init; }
    public decimal SubTotal { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal AmountPaid { get; init; }
    public decimal Change { get; init; }
    public bool ShowWatermark { get; init; }
    public IReadOnlyList<SaleReceiptLine> Lines { get; init; } = Array.Empty<SaleReceiptLine>();
}

public sealed class SaleReceiptLine
{
    public string Name { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Discount { get; init; }
    public decimal LineTotal { get; init; }
}
