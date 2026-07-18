using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class ReturnRepository : Repository<Return>, IReturnRepository
{
    public ReturnRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Return?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.OriginalSale)
            .Include(x => x.ProcessedBy)
            .Include(x => x.Items)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Return>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.OriginalSaleId == saleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
