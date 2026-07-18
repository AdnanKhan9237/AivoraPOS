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
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IReturnRepository, ReturnRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppSettingRepository, AppSettingRepository>();
        services.AddScoped<ILicenseRecordRepository, LicenseRecordRepository>();

        services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        return services;
    }
}
