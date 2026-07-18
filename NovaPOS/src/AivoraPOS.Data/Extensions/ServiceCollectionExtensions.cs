using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Data.Repositories;
using AivoraPOS.Data.Services;

namespace AivoraPOS.Data.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAivoraPOSData(this IServiceCollection services)
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
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<IInventoryAlertService, InventoryAlertService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductImportService, ProductImportService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IDataManagementService, DataManagementService>();
        services.AddScoped<IUserAdminService, UserAdminService>();

        return services;
    }
}
