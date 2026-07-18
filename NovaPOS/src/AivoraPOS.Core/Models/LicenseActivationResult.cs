using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models;

public sealed class LicenseActivationResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public LicensePlan? Plan { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
