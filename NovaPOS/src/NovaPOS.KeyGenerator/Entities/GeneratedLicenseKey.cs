using NovaPOS.Core.Enums;

namespace NovaPOS.KeyGenerator.Entities;

public class GeneratedLicenseKey
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public LicensePlan Plan { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}
