using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Data.Repositories;
using NovaPOS.Data.Services;

namespace NovaPOS.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSData(this IServiceCollection services)
    {
        services.AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionFactory = serviceProvider.GetRequiredService<IDatabaseConnectionFactory>();
            options.UseSqlite(connectionFactory.GetConnectionString());
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        services.AddScoped<ILicenseInfoRepository, LicenseInfoRepository>();

        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
