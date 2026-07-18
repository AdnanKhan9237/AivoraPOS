using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
}
