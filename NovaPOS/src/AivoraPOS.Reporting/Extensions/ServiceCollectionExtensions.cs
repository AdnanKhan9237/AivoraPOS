using Microsoft.Extensions.DependencyInjection;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Reporting.Services;

namespace AivoraPOS.Reporting.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAivoraPOSReporting(this IServiceCollection services)
    {
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        return services;
    }
}
