using Microsoft.Extensions.DependencyInjection;

namespace NovaPOS.Reporting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSReporting(this IServiceCollection services)
    {
        return services;
    }
}
