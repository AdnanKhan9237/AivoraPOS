using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public StockMovementType MovementType { get; set; }
    public decimal QuantityChange { get; set; }
    public decimal QuantityAfter { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int UserId { get; set; }
    public string? Notes { get; set; }

    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
