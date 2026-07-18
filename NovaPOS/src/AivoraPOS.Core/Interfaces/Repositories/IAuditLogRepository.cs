using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> SearchAsync(
        DateTime? startUtc,
        DateTime? endUtc,
        Guid? userId,
        string? actionContains,
        CancellationToken cancellationToken = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}
