using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Entities;

public class LicenseInfo
{
    public int Id { get; set; }
    public string LicenseKey { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public DateTime ActivatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string HardwareFingerprint { get; set; } = string.Empty;
    public LicensePlan Plan { get; set; }
    public bool IsValid { get; set; }
}
