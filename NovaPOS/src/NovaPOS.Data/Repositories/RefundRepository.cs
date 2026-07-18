using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class RefundRepository : Repository<Refund>, IRefundRepository
{
    public RefundRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Refund?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.OriginalSale)
            .Include(x => x.ProcessedBy)
            .Include(x => x.Items)
            .ThenInclude(x => x.SaleItem)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Refund>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.OriginalSaleId == saleId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
