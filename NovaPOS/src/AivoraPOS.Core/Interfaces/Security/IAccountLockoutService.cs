using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Security;

public interface IAccountLockoutService
{
    Task<bool> IsPinLockedOutAsync(User user, CancellationToken cancellationToken = default);
    Task RegisterFailedPinAttemptAsync(User user, CancellationToken cancellationToken = default);
    Task ResetPinAttemptsAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> IsPasswordLockedOutAsync(User user, CancellationToken cancellationToken = default);
    Task RegisterFailedPasswordAttemptAsync(User user, CancellationToken cancellationToken = default);
    Task ResetPasswordAttemptsAsync(User user, CancellationToken cancellationToken = default);
    Task UnlockAccountAsync(User user, CancellationToken cancellationToken = default);
}
