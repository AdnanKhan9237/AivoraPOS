namespace NovaPOS.Core.Entities;

public class Product : BaseEntity
{
    public int CategoryId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackInventory { get; set; } = true;

    public Category Category { get; set; } = null!;
    public Inventory? Inventory { get; set; }
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
