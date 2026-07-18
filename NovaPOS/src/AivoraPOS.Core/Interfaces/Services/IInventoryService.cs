using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Models.Products;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IInventoryService
{
    Task<Product> AdjustStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InventoryMovement>> GetMovementHistoryAsync(Guid productId, CancellationToken cancellationToken = default);
}
