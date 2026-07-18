namespace NovaPOS.Core.Entities;

public class Inventory : BaseEntity
{
    public int ProductId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal ReorderLevel { get; set; }
    public DateTime? LastRestockedAt { get; set; }

    public Product Product { get; set; } = null!;
}
