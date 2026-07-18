using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Services;

public interface IInventoryAlertService
{
    int LowStockCount { get; }
    event EventHandler? LowStockCountChanged;
    Task RefreshAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
}
