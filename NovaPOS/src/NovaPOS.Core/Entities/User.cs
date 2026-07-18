using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }

    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<Return> ProcessedReturns { get; set; } = new List<Return>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
