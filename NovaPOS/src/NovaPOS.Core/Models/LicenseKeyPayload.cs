using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models;

public sealed class LicenseKeyPayload
{
    public LicensePlan Plan { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public uint Salt { get; init; }
}
