using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

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
}
