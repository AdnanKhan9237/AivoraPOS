namespace NovaPOS.Core.Entities;

public class SaleItem : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }

    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ICollection<ReturnItem> ReturnItems { get; set; } = new List<ReturnItem>();
}
