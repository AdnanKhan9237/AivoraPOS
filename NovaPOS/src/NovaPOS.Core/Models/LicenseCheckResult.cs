using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models;

public sealed class LicenseCheckResult
{
    public LicenseStatus Status { get; init; }
    public LicensePlan EffectivePlan { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsTrial { get; init; }
    public int? TrialDaysRemaining { get; init; }
    public string Message { get; init; } = string.Empty;
    public bool CanRunApplication { get; init; }
    public bool CanUseFeatures { get; init; }
    public bool IsReadOnlyMode { get; init; }
}
