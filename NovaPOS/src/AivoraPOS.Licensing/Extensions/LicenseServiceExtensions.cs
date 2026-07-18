using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Licensing;
using AivoraPOS.Licensing;

namespace AivoraPOS.Licensing.Extensions;

public static class LicenseServiceExtensions
{
    public static void RequireFeature(this ILicenseService licenseService, LicenseFeature feature)
    {
        LicenseGuard.EnsureCanUse(licenseService, feature);
    }
}
