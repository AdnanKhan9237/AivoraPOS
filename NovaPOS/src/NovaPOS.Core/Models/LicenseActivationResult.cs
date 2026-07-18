using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models;

public sealed class LicenseActivationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public LicensePlan? Plan { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
