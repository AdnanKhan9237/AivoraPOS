using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Models;

public sealed class AuthResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public User? User { get; init; }
    public bool IsLockedOut { get; init; }
    public DateTime? LockedUntilUtc { get; init; }

    public static AuthResult Succeeded(User user, string message = "Login successful.") =>
        new() { Success = true, Message = message, User = user };

    public static AuthResult Failed(string message, bool isLockedOut = false, DateTime? lockedUntilUtc = null) =>
        new() { Success = false, Message = message, IsLockedOut = isLockedOut, LockedUntilUtc = lockedUntilUtc };
}
