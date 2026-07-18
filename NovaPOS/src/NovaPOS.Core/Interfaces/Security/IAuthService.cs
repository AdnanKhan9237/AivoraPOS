using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Interfaces.Security;

public interface IAuthService
{
    Task<Models.AuthResult> LoginWithPinAsync(Guid userId, string pin, CancellationToken cancellationToken = default);
    Task<Models.AuthResult> LoginWithPasswordAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<Models.AuthResult> ManagerOverrideAsync(string username, string password, CancellationToken cancellationToken = default);
    Task LogoutAsync(CancellationToken cancellationToken = default);
    Task<bool> UnlockAccountAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default);
}
