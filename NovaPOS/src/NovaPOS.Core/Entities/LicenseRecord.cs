namespace NovaPOS.Core.Entities;

public class LicenseRecord : BaseEntity
{
    public string LicenseKeyHash { get; set; } = string.Empty;
    public string HardwareFingerprint { get; set; } = string.Empty;
    public DateTime ActivatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastValidatedAt { get; set; }
    public string? CompanyName { get; set; }
}
