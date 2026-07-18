namespace NovaPOS.Core.Interfaces.Security;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string? entityType = null,
        string? entityId = null,
        string? oldValues = null,
        string? newValues = null,
        Guid? userId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
