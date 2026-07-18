using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class InventoryMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public InventoryMovementType MovementType { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityChange { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reference { get; set; }
    public Guid UserId { get; set; }

    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
}
