using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Data.Services;

public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IAppSettingRepository _appSettingRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IAppSettingRepository appSettingRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        IInventoryAlertService inventoryAlertService,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _appSettingRepository = appSettingRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _inventoryAlertService = inventoryAlertService;
        _logger = logger;
    }

    public Task<PagedResult<Product>> GetProductsPagedAsync(ProductListQuery query, CancellationToken cancellationToken = default) =>
        _productRepository.SearchPagedAsync(query, cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _productRepository.GetByIdAsync(id, cancellationToken);

    public async Task<Product> SaveAsync(ProductSaveRequest request, CancellationToken cancellationToken = default)
    {
        ValidateSaveRequest(request);

        if (!await _productRepository.IsSkuUniqueAsync(request.Sku, request.Id, cancellationToken))
        {
            throw new InvalidOperationException($"SKU '{request.Sku}' is already in use.");
        }

        Product product;
        if (request.Id.HasValue)
        {
            product = await _productRepository.GetByIdAsync(request.Id.Value, cancellationToken)
                ?? throw new InvalidOperationException("Product was not found.");

            var previousStock = product.StockQuantity;
            product.Name = request.Name.Trim();
            product.Sku = request.Sku.Trim();
            product.Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
            product.CategoryId = request.CategoryId;
            product.PurchasePrice = request.PurchasePrice;
            product.SalePrice = request.SalePrice;
            product.TaxRate = request.TaxRate;
            product.LowStockThreshold = request.LowStockThreshold;
            product.IsActive = request.IsActive;
            product.StockQuantity = request.StockQuantity;

            await _productRepository.UpdateAsync(product, cancellationToken);

            if (previousStock != request.StockQuantity)
            {
                await _inventoryMovementRepository.AddAsync(new InventoryMovement
                {
                    ProductId = product.Id,
                    MovementType = InventoryMovementType.ManualAdjust,
                    QuantityBefore = previousStock,
                    QuantityChange = request.StockQuantity - previousStock,
                    QuantityAfter = request.StockQuantity,
                    Reference = "Product edit stock update",
                    UserId = request.UserId
                }, cancellationToken);
            }
        }
        else
        {
            product = new Product
            {
                Name = request.Name.Trim(),
                Sku = request.Sku.Trim(),
                Barcode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim(),
                CategoryId = request.CategoryId,
                PurchasePrice = request.PurchasePrice,
                SalePrice = request.SalePrice,
                TaxRate = request.TaxRate,
                StockQuantity = request.StockQuantity,
                LowStockThreshold = request.LowStockThreshold,
                IsActive = request.IsActive
            };

            await _productRepository.AddAsync(product, cancellationToken);

            if (request.StockQuantity > 0)
            {
                await _inventoryMovementRepository.AddAsync(new InventoryMovement
                {
                    ProductId = product.Id,
                    MovementType = InventoryMovementType.InitialStock,
                    QuantityBefore = 0,
                    QuantityChange = request.StockQuantity,
                    QuantityAfter = request.StockQuantity,
                    Reference = "Initial stock",
                    UserId = request.UserId
                }, cancellationToken);
            }
        }

        await _productRepository.SaveChangesAsync(cancellationToken);
        await _inventoryAlertService.RefreshAsync(cancellationToken);
        _logger.LogInformation("Saved product {Sku} ({ProductId}).", product.Sku, product.Id);
        return product;
    }

    public async Task SoftDeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException("Product was not found.");

        product.IsActive = false;
        await _productRepository.UpdateAsync(product, cancellationToken);
        await _productRepository.SaveChangesAsync(cancellationToken);
        await _inventoryAlertService.RefreshAsync(cancellationToken);
        _logger.LogInformation("Deactivated product {Sku} ({ProductId}) by user {UserId}.", product.Sku, product.Id, userId);
    }

    public Task<bool> IsSkuUniqueAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default) =>
        _productRepository.IsSkuUniqueAsync(sku, excludeProductId, cancellationToken);

    public async Task<string> GenerateSkuAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var candidate = $"SKU-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
            if (await _productRepository.IsSkuUniqueAsync(candidate, cancellationToken: cancellationToken))
            {
                return candidate;
            }
        }

        return $"SKU-{Guid.NewGuid():N}"[..20].ToUpperInvariant();
    }

    public async Task<decimal> GetDefaultTaxRateAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _appSettingRepository.GetByKeyAsync("Tax.DefaultRate", cancellationToken);
        return setting is not null && decimal.TryParse(setting.Value, out var rate) ? rate : 0.0825m;
    }

    private static void ValidateSaveRequest(ProductSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Sku))
        {
            throw new InvalidOperationException("SKU is required.");
        }

        if (request.CategoryId == Guid.Empty)
        {
            throw new InvalidOperationException("Category is required.");
        }

        if (request.PurchasePrice < 0 || request.SalePrice < 0)
        {
            throw new InvalidOperationException("Prices must be positive values.");
        }

        if (request.StockQuantity < 0 || request.LowStockThreshold < 0)
        {
            throw new InvalidOperationException("Stock values cannot be negative.");
        }
    }
}
