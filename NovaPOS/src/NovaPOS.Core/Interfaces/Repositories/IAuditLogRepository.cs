using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}
