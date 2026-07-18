using Microsoft.Extensions.Logging;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Models;

namespace AivoraPOS.Security.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountLockoutService _accountLockoutService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IAccountLockoutService accountLockoutService,
        IAuditService auditService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _accountLockoutService = accountLockoutService;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<AuthResult> LoginWithPinAsync(Guid userId, string pin, CancellationToken cancellationToken = default)
    {
        if (!PasswordPolicy.IsPinAllowed(pin))
        {
            return AuthResult.Failed("This PIN is not allowed. Choose a different PIN.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await _auditService.LogAsync("User.LoginFailed", "User", userId.ToString(), cancellationToken: cancellationToken);
            return AuthResult.Failed("Invalid cashier selection or inactive account.");
        }

        if (await _accountLockoutService.IsPinLockedOutAsync(user, cancellationToken))
        {
            return AuthResult.Failed(
                $"PIN locked due to too many failed attempts. Try again after {user.PinLockedUntilUtc:HH:mm} UTC.",
                isLockedOut: true,
                lockedUntilUtc: user.PinLockedUntilUtc);
        }

        if (!_passwordHasher.VerifyPin(pin, user.PinHash))
        {
            await _accountLockoutService.RegisterFailedPinAttemptAsync(user, cancellationToken);
            await _auditService.LogAsync("User.LoginFailed", "User", user.Id.ToString(), userId: user.Id, cancellationToken: cancellationToken);
            return AuthResult.Failed("Incorrect PIN.");
        }

        await CompleteSuccessfulLoginAsync(user, cancellationToken);
        return AuthResult.Succeeded(user);
    }

    public async Task<AuthResult> LoginWithPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !user.IsActive)
        {
            await _auditService.LogAsync("User.LoginFailed", "User", username, cancellationToken: cancellationToken);
            return AuthResult.Failed("Invalid username or password.");
        }

        if (user.Role == UserRole.Cashier)
        {
            return AuthResult.Failed("Cashiers must sign in with a PIN.");
        }

        if (await _accountLockoutService.IsPasswordLockedOutAsync(user, cancellationToken))
        {
            return AuthResult.Failed("Account is locked. Contact an administrator to unlock it.", isLockedOut: true);
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            await _accountLockoutService.RegisterFailedPasswordAttemptAsync(user, cancellationToken);
            await _auditService.LogAsync("User.LoginFailed", "User", user.Id.ToString(), userId: user.Id, cancellationToken: cancellationToken);
            return AuthResult.Failed("Invalid username or password.");
        }

        await CompleteSuccessfulLoginAsync(user, cancellationToken);
        return AuthResult.Succeeded(user);
    }

    public async Task<AuthResult> ManagerOverrideAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null || !user.IsActive || user.Role == UserRole.Cashier)
        {
            return AuthResult.Failed("Manager approval denied.");
        }

        if (await _accountLockoutService.IsPasswordLockedOutAsync(user, cancellationToken))
        {
            return AuthResult.Failed("Manager account is locked.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            await _accountLockoutService.RegisterFailedPasswordAttemptAsync(user, cancellationToken);
            await _auditService.LogAsync(
                "User.ManagerOverrideFailed",
                "User",
                user.Id.ToString(),
                userId: _currentUserService.CurrentUser?.Id,
                cancellationToken: cancellationToken);
            return AuthResult.Failed("Manager approval denied.");
        }

        await _auditService.LogAsync(
            "User.ManagerOverride",
            "User",
            user.Id.ToString(),
            newValues: new { Manager = user.Username },
            userId: _currentUserService.CurrentUser?.Id,
            cancellationToken: cancellationToken);

        return AuthResult.Succeeded(user, "Manager approval granted.");
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.CurrentUser?.Id;
        _currentUserService.Clear();

        if (userId is not null)
        {
            await _auditService.LogAsync("User.Logout", "User", userId.ToString(), userId: userId, cancellationToken: cancellationToken);
        }
    }

    public async Task<bool> UnlockAccountAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var admin = await _userRepository.GetByIdAsync(adminUserId, cancellationToken);
        if (admin is null || admin.Role != UserRole.Admin)
        {
            return false;
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await _accountLockoutService.UnlockAccountAsync(user, cancellationToken);
        await _auditService.LogAsync(
            "User.AccountUnlocked",
            "User",
            user.Id.ToString(),
            userId: adminUserId,
            cancellationToken: cancellationToken);

        return true;
    }

    private async Task CompleteSuccessfulLoginAsync(User user, CancellationToken cancellationToken)
    {
        user.LastLoginAt = DateTime.UtcNow;
        await _accountLockoutService.ResetPinAttemptsAsync(user, cancellationToken);
        await _accountLockoutService.ResetPasswordAttemptsAsync(user, cancellationToken);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        _currentUserService.SetCurrentUser(user);
        await _auditService.LogAsync("User.Login", "User", user.Id.ToString(), userId: user.Id, cancellationToken: cancellationToken);
        _logger.LogInformation("User {Username} logged in.", user.Username);
    }
}
