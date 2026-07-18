using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Models;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Data.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Sku == sku, cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.CategoryId == categoryId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.Trim();

        return await DbSet
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsActive && (
                x.Name.Contains(term) ||
                x.Sku.Contains(term) ||
                (x.Barcode != null && x.Barcode.Contains(term))))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.IsActive && x.StockQuantity <= x.LowStockThreshold)
            .OrderBy(x => x.StockQuantity)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveCatalogAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> SearchPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default)
    {
        var products = DbSet.AsNoTracking().Include(x => x.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            products = products.Where(x =>
                x.Name.Contains(term) ||
                x.Sku.Contains(term) ||
                (x.Barcode != null && x.Barcode.Contains(term)));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        products = query.Status switch
        {
            ProductStatusFilter.Active => products.Where(x => x.IsActive),
            ProductStatusFilter.Inactive => products.Where(x => !x.IsActive),
            _ => products
        };

        products = query.StockLevel switch
        {
            StockLevelFilter.Low => products.Where(x => x.IsActive && x.StockQuantity <= x.LowStockThreshold),
            StockLevelFilter.OutOfStock => products.Where(x => x.StockQuantity <= 0),
            StockLevelFilter.Adequate => products.Where(x => x.StockQuantity > x.LowStockThreshold),
            _ => products
        };

        var totalCount = await products.CountAsync(cancellationToken);
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, query.PageSize);

        var items = await products
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var normalized = sku.Trim();
        var query = DbSet.Where(x => x.Sku == normalized);

        if (excludeProductId.HasValue)
        {
            query = query.Where(x => x.Id != excludeProductId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<int> CountLowStockAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .CountAsync(x => x.IsActive && x.StockQuantity <= x.LowStockThreshold, cancellationToken);
    }

    public async Task<int> CountActiveProductsInCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .CountAsync(x => x.CategoryId == categoryId && x.IsActive, cancellationToken);
    }
}
