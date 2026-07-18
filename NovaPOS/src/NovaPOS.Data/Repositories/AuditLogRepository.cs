using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(int count, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc && x.CreatedAt <= endUtc)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public override Task DeleteAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Audit logs cannot be deleted.");
    }
}
