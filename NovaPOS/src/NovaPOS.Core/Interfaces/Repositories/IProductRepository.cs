using NovaPOS.Core.Entities;
using NovaPOS.Core.Models;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveCatalogAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> SearchPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<int> CountLowStockAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveProductsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
