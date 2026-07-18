using NovaPOS.Core.Enums;
using NovaPOS.Core.Exceptions;
using NovaPOS.Core.Interfaces.Licensing;

namespace NovaPOS.Licensing;

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
