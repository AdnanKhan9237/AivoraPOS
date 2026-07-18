using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models;

public sealed class LicenseKeyPayload
{
    public LicensePlan Plan { get; init; }
    public DateTime ExpiresAtUtc { get; init; }
    public uint Salt { get; init; }
}
