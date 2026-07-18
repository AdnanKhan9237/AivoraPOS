using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveCatalogAsync(CancellationToken cancellationToken = default);
}
