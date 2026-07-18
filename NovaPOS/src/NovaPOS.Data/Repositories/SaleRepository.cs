using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class SaleRepository : Repository<Sale>, ISaleRepository
{
    public SaleRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Cashier)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Cashier)
            .Where(x => x.CreatedAt >= startUtc && x.CreatedAt <= endUtc)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
