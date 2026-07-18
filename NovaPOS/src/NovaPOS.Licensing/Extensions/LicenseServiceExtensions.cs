using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Licensing;

namespace NovaPOS.Licensing.Extensions;

public static class LicenseServiceExtensions
{
    public static void RequireFeature(this ILicenseService licenseService, LicenseFeature feature)
    {
        LicenseGuard.EnsureCanUse(licenseService, feature);
    }
}
