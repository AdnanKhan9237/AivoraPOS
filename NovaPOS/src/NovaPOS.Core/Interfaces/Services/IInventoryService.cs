using NovaPOS.Core.Entities;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<Product> AdjustStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryMovement>> GetMovementHistoryAsync(Guid productId, CancellationToken cancellationToken = default);
}
