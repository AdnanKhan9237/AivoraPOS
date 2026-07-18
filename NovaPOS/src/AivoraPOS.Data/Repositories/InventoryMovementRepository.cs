using Microsoft.EntityFrameworkCore;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;

namespace AivoraPOS.Data.Repositories;

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
