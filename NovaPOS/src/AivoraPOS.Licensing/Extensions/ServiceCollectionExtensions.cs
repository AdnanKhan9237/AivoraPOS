using Microsoft.Extensions.DependencyInjection;
using AivoraPOS.Core.Interfaces.Licensing;

namespace AivoraPOS.Licensing.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAivoraPOSLicensing(this IServiceCollection services)
    {
        services.AddSingleton<LicenseCache>();
        services.AddHttpClient(nameof(LicenseService));
        services.AddScoped<ILicenseService, LicenseService>();
        return services;
    }
}
