using NovaPOS.Core.Entities;
using NovaPOS.Core.Models;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Core.Interfaces.Services;

public interface IProductService
{
    Task<PagedResult<Product>> GetProductsPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Product> SaveAsync(ProductSaveRequest request, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<string> GenerateSkuAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetDefaultTaxRateAsync(CancellationToken cancellationToken = default);
}
