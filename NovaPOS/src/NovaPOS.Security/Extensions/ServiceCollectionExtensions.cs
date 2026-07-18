using Microsoft.Extensions.DependencyInjection;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Security.Audit;
using NovaPOS.Security.Auth;
using NovaPOS.Security.Cryptography;
using NovaPOS.Security.Integrity;
using NovaPOS.Security.Session;
using NovaPOS.Security.Settings;

namespace NovaPOS.Security.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNovaPOSSecurity(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, AesEncryptionService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISessionTimeoutService, SessionTimeoutService>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();

        services.AddScoped<IAccountLockoutService, AccountLockoutService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAppIntegrityService, AppIntegrityService>();
        services.AddScoped<SensitiveSettingService>();

        return services;
    }
}
