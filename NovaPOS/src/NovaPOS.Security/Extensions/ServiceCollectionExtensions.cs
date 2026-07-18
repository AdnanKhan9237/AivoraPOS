using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Security.Audit;
using NovaPOS.Security.Cryptography;

namespace NovaPOS.Security.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
