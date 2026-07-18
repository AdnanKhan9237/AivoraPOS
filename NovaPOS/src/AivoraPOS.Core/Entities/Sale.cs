using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public string? Notes { get; set; }

    public User Cashier { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
