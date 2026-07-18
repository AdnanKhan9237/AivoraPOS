using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;

namespace NovaPOS.Security.Audit;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAuditLogRepository auditLogRepository, ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        string? entityId = null,
        string? details = null,
        int? userId = null,
        string? username = null,
        CancellationToken cancellationToken = default)
    {
        var entry = new AuditLog
        {
            UserId = userId,
            Username = username ?? "system",
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details
        };

        await _auditLogRepository.AddAsync(entry, cancellationToken);
        await _auditLogRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Audit: {Action} on {EntityType} ({EntityId}) by {Username}",
            action,
            entityType,
            entityId,
            entry.Username);
    }
}
