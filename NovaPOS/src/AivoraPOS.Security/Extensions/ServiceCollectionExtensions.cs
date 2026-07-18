using Microsoft.Extensions.DependencyInjection;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Security.Audit;
using AivoraPOS.Security.Auth;
using AivoraPOS.Security.Cryptography;
using AivoraPOS.Security.Integrity;
using AivoraPOS.Security.Session;
using AivoraPOS.Security.Settings;

namespace AivoraPOS.Security.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAivoraPOSSecurity(this IServiceCollection services)
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
