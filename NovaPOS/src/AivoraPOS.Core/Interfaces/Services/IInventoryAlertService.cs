using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IInventoryAlertService
{
    int LowStockCount { get; }
    event EventHandler? LowStockCountChanged;
    Task RefreshAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
}
