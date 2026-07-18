using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Data.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IProductRepository productRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        IInventoryAlertService inventoryAlertService,
        ILogger<InventoryService> logger)
    {
        _productRepository = productRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _inventoryAlertService = inventoryAlertService;
        _logger = logger;
    }

    public async Task<Product> AdjustStockAsync(StockAdjustmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.NewQuantity < 0)
        {
            throw new InvalidOperationException("Stock quantity cannot be negative.");
        }

        if (request.Reason != StockAdjustmentReason.Restock && string.IsNullOrWhiteSpace(request.Notes))
        {
            throw new InvalidOperationException("Notes are required for this stock adjustment.");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException("Product was not found.");

        var quantityBefore = product.StockQuantity;
        var quantityChange = request.NewQuantity - quantityBefore;

        if (quantityChange == 0)
        {
            return product;
        }

        product.StockQuantity = request.NewQuantity;
        await _productRepository.UpdateAsync(product, cancellationToken);

        var reference = BuildReference(request.Reason, request.Notes);
        await _inventoryMovementRepository.AddAsync(new InventoryMovement
        {
            ProductId = product.Id,
            MovementType = request.Reason == StockAdjustmentReason.Restock
                ? InventoryMovementType.InitialStock
                : InventoryMovementType.ManualAdjust,
            QuantityBefore = quantityBefore,
            QuantityChange = quantityChange,
            QuantityAfter = request.NewQuantity,
            Reference = reference,
            UserId = request.UserId
        }, cancellationToken);

        await _productRepository.SaveChangesAsync(cancellationToken);
        await _inventoryAlertService.RefreshAsync(cancellationToken);
        _logger.LogInformation(
            "Adjusted stock for {Sku} from {Before} to {After} ({Reason}).",
            product.Sku,
            quantityBefore,
            request.NewQuantity,
            request.Reason);

        return product;
    }

    public Task<IReadOnlyList<InventoryMovement>> GetMovementHistoryAsync(Guid productId, CancellationToken cancellationToken = default) =>
        _inventoryMovementRepository.GetByProductIdAsync(productId, cancellationToken);

    private static string BuildReference(StockAdjustmentReason reason, string? notes)
    {
        var reasonText = reason.ToString();
        if (string.IsNullOrWhiteSpace(notes))
        {
            return reasonText;
        }

        var combined = $"{reasonText}: {notes.Trim()}";
        return combined.Length <= 200 ? combined : combined[..200];
    }
}
