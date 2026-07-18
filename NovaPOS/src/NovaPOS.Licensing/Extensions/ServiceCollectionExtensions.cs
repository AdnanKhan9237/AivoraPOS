using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Licensing;

namespace NovaPOS.Licensing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSLicensing(this IServiceCollection services)
    {
        services.AddSingleton<LicenseCache>();
        services.AddHttpClient(nameof(LicenseService));
        services.AddScoped<ILicenseService, LicenseService>();
        return services;
    }
}
