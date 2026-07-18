using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Refund> ProcessedRefunds { get; set; } = new List<Refund>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
