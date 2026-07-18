namespace NovaPOS.Core.Entities;

public class RefundItem : BaseEntity
{
    public Guid RefundId { get; set; }
    public Guid SaleItemId { get; set; }
    public int QuantityReturned { get; set; }
    public decimal RefundAmount { get; set; }

    public Refund Refund { get; set; } = null!;
    public SaleItem SaleItem { get; set; } = null!;
}
