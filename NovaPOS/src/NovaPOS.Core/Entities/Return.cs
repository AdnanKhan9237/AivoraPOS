using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class Return : BaseEntity
{
    public string ReturnNumber { get; set; } = string.Empty;
    public int OriginalSaleId { get; set; }
    public int ProcessedById { get; set; }
    public ReturnStatus Status { get; set; } = ReturnStatus.Pending;
    public string? Reason { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentMethod RefundMethod { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Sale OriginalSale { get; set; } = null!;
    public User ProcessedBy { get; set; } = null!;
    public ICollection<ReturnItem> Items { get; set; } = new List<ReturnItem>();
}
