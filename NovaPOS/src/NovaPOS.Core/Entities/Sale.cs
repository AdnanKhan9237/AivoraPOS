using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public int CashierId { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Pending;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? CompletedAt { get; set; }

    public User Cashier { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Return> Returns { get; set; } = new List<Return>();
}
