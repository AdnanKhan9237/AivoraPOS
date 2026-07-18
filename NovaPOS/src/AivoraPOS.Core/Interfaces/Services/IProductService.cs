using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Models;
using AivoraPOS.Core.Models.Products;

namespace AivoraPOS.Core.Interfaces.Services;

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
