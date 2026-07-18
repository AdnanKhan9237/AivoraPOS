using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Inventory>> GetLowStockAsync(CancellationToken cancellationToken = default);
}
