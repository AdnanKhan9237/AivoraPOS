namespace NovaPOS.Core.Entities;

public class ReturnItem : BaseEntity
{
    public int ReturnId { get; set; }
    public int OriginalSaleItemId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public Return Return { get; set; } = null!;
    public SaleItem OriginalSaleItem { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
