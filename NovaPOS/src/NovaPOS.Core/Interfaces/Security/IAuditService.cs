namespace NovaPOS.Core.Interfaces.Security;

public interface IAuditService
{
    Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? details = null,
        int? userId = null,
        string? username = null,
        CancellationToken cancellationToken = default);
}
