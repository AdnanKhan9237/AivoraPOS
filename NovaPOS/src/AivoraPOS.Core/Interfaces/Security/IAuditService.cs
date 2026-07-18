using AivoraPOS.Core.Enums;

namespace AivoraPOS.Core.Interfaces.Security;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        object? oldValues = null,
        object? newValues = null,
        Guid? userId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
