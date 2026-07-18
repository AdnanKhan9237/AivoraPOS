using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Auth;

public sealed class AccountLockoutService : IAccountLockoutService
{
    public const int MaxFailedAttempts = 5;
    public static readonly TimeSpan PinLockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountLockoutService> _logger;

    public AccountLockoutService(IUserRepository userRepository, ILogger<AccountLockoutService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public Task<bool> IsPinLockedOutAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.PinLockedUntilUtc is null)
        {
            return Task.FromResult(false);
        }

        if (user.PinLockedUntilUtc <= DateTime.UtcNow)
        {
            user.PinLockedUntilUtc = null;
            user.FailedPinAttempts = 0;
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public async Task RegisterFailedPinAttemptAsync(User user, CancellationToken cancellationToken = default)
    {
        user.FailedPinAttempts++;
        if (user.FailedPinAttempts >= MaxFailedAttempts)
        {
            user.PinLockedUntilUtc = DateTime.UtcNow.Add(PinLockoutDuration);
            _logger.LogWarning("PIN lockout applied for user {UserId} until {LockedUntil}.", user.Id, user.PinLockedUntilUtc);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPinAttemptsAsync(User user, CancellationToken cancellationToken = default)
    {
        user.FailedPinAttempts = 0;
        user.PinLockedUntilUtc = null;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> IsPasswordLockedOutAsync(User user, CancellationToken cancellationToken = default) =>
        Task.FromResult(user.IsAccountLocked);

    public async Task RegisterFailedPasswordAttemptAsync(User user, CancellationToken cancellationToken = default)
    {
        user.FailedPasswordAttempts++;
        if (user.FailedPasswordAttempts >= MaxFailedAttempts)
        {
            user.IsAccountLocked = true;
            _logger.LogWarning("Account locked for user {UserId} after failed password attempts.", user.Id);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAttemptsAsync(User user, CancellationToken cancellationToken = default)
    {
        user.FailedPasswordAttempts = 0;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAccountAsync(User user, CancellationToken cancellationToken = default)
    {
        user.IsAccountLocked = false;
        user.FailedPasswordAttempts = 0;
        user.FailedPinAttempts = 0;
        user.PinLockedUntilUtc = null;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
