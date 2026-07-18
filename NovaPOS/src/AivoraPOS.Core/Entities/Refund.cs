namespace AivoraPOS.Core.Entities;

public class Refund : BaseEntity
{
    public Guid OriginalSaleId { get; set; }
    public Guid ProcessedById { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }

    public Sale OriginalSale { get; set; } = null!;
    public User ProcessedBy { get; set; } = null!;
    public ICollection<RefundItem> Items { get; set; } = new List<RefundItem>();
}
