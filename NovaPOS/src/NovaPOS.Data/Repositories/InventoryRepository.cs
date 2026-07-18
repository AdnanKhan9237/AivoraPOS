using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Product)
            .Where(x => x.Product.IsActive && x.QuantityOnHand <= x.ReorderLevel)
            .OrderBy(x => x.QuantityOnHand)
            .ToListAsync(cancellationToken);
    }
}
