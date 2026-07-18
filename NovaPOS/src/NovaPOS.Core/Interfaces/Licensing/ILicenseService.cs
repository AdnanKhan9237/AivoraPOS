using NovaPOS.Core.Enums;
using NovaPOS.Core.Models;

namespace NovaPOS.Core.Interfaces.Licensing;

public interface ILicenseService
{
    LicenseStatus CurrentStatus { get; }
    LicensePlan EffectivePlan { get; }
    bool IsTrial { get; }
    int? MaxProducts { get; }
    int? MaxCashiers { get; }
    bool ShowReceiptWatermark { get; }
    int? TrialDaysRemaining { get; }
    DateTime? ExpiresAt { get; }

    Task<LicenseCheckResult> ValidateOnLaunchAsync(CancellationToken cancellationToken = default);
    Task<LicenseActivationResult> ActivateAsync(string licenseKey, string businessName, CancellationToken cancellationToken = default);
    bool CanUse(LicenseFeature feature);
}
