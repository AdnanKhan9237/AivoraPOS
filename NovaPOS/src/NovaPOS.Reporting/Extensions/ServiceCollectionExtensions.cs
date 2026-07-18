using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Reporting.Services;

namespace NovaPOS.Reporting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSReporting(this IServiceCollection services)
    {
        services.AddScoped<IReceiptService, ReceiptService>();
        return services;
    }
}
