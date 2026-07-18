namespace NovaPOS.Core.Models.Sales;

public sealed class CartLine
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public decimal TaxRate { get; init; }
    public int Quantity { get; init; }
    public decimal LineDiscount { get; init; }

    public decimal NetAmount => UnitPrice * Quantity - LineDiscount;
    public decimal TaxAmount => Math.Round(NetAmount * TaxRate, 2, MidpointRounding.AwayFromZero);
    public decimal LineTotal => NetAmount + TaxAmount;
}
