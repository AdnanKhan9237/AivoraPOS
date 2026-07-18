using Microsoft.Extensions.Logging;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.Data.Services;

public sealed class UserAdminService : IUserAdminService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;
    private readonly ILogger<UserAdminService> _logger;

    public UserAdminService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAuditService auditService,
        ILogger<UserAdminService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
        _logger = logger;
    }

    public Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken = default) =>
        _userRepository.GetAllUsersAsync(cancellationToken);

    public async Task<User> CreateUserAsync(
        string fullName,
        string username,
        UserRole role,
        string pin,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException("Full name and username are required.");
        }

        if (await _userRepository.UsernameExistsAsync(username, cancellationToken))
        {
            throw new InvalidOperationException($"Username '{username}' is already in use.");
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Username = username.Trim(),
            Role = role,
            PinHash = _passwordHasher.HashPin(pin),
            PasswordHash = _passwordHasher.HashPassword(password),
            IsActive = true
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("UserCreated", "User", user.Id.ToString(), newValues: new { user.FullName, user.Username, user.Role }, cancellationToken: cancellationToken);
        _logger.LogInformation("Created user {Username}.", user.Username);
        return user;
    }

    public async Task<User> UpdateUserAsync(
        Guid userId,
        string fullName,
        UserRole role,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User was not found.");

        user.FullName = fullName.Trim();
        user.Role = role;
        user.IsActive = isActive;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("UserUpdated", "User", user.Id.ToString(), newValues: new { user.FullName, user.Role, user.IsActive }, cancellationToken: cancellationToken);
        return user;
    }

    public async Task ResetPinAsync(Guid userId, string newPin, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User was not found.");

        user.PinHash = _passwordHasher.HashPin(newPin);
        user.FailedPinAttempts = 0;
        user.PinLockedUntilUtc = null;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("UserPinReset", "User", user.Id.ToString(), cancellationToken: cancellationToken);
    }

    public async Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User was not found.");

        user.IsActive = false;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("UserDeactivated", "User", user.Id.ToString(), cancellationToken: cancellationToken);
    }

    public async Task ChangeOwnPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("User was not found.");

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("PasswordChanged", "User", user.Id.ToString(), cancellationToken: cancellationToken);
    }
}
