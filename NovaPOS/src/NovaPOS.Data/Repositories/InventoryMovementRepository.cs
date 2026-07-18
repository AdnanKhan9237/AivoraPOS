using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class InventoryMovementRepository : Repository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
