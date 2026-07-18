using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context) : base(context)
    {
        _context = context;
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

    public async Task<IReadOnlyList<AuditLog>> SearchAsync(
        DateTime? startUtc,
        DateTime? endUtc,
        Guid? userId,
        string? actionContains,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsNoTracking().AsQueryable();

        if (startUtc is not null)
        {
            query = query.Where(x => x.CreatedAt >= startUtc);
        }

        if (endUtc is not null)
        {
            query = query.Where(x => x.CreatedAt <= endUtc);
        }

        if (userId is not null)
        {
            query = query.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(actionContains))
        {
            query = query.Where(x => x.Action.Contains(actionContains));
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(500)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .Where(x => x.CreatedAt < cutoffUtc)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public override Task DeleteAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Audit logs cannot be deleted.");
    }
}
