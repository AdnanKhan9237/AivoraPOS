using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Exceptions;
using AivoraPOS.Core.Interfaces.Licensing;

namespace AivoraPOS.Licensing;

public static class LicenseGuard
{
    public static void EnsureCanUse(ILicenseService licenseService, LicenseFeature feature)
    {
        if (!licenseService.CanUse(feature))
        {
            throw new LicenseRestrictionException(
                $"Upgrade to a higher plan to use {feature}. Current plan: {licenseService.EffectivePlan}.");
        }
    }
}
