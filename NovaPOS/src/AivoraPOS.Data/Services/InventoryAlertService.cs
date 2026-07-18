using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.Data.Services;

public sealed class InventoryAlertService : IInventoryAlertService
{
    private readonly IProductRepository _productRepository;

    public InventoryAlertService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public int LowStockCount { get; private set; }

    public event EventHandler? LowStockCountChanged;

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        var count = await _productRepository.CountLowStockAsync(cancellationToken);
        if (count != LowStockCount)
        {
            LowStockCount = count;
            LowStockCountChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public Task<IReadOnlyList<Product>> GetLowStockProductsAsync(CancellationToken cancellationToken = default) =>
        _productRepository.GetLowStockAsync(cancellationToken);
}
