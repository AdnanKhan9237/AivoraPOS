using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Licensing.Services;

namespace NovaPOS.Licensing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSLicensing(this IServiceCollection services)
    {
        services.AddSingleton<IHardwareFingerprintService, HardwareFingerprintService>();
        services.AddScoped<ILicenseValidationService, LicenseValidationService>();
        return services;
    }
}
