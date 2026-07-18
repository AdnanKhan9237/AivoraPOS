using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Sales;

namespace NovaPOS.Data.Services;

public sealed class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryMovementRepository _inventoryMovementRepository;
    private readonly IAppSettingRepository _appSettingRepository;
    private readonly IReceiptService _receiptService;
    private readonly IAuditService _auditService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IInventoryMovementRepository inventoryMovementRepository,
        IAppSettingRepository appSettingRepository,
        IReceiptService receiptService,
        IAuditService auditService,
        IUserRepository userRepository,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _inventoryMovementRepository = inventoryMovementRepository;
        _appSettingRepository = appSettingRepository;
        _receiptService = receiptService;
        _auditService = auditService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<string> GenerateNextSaleNumberAsync(CancellationToken cancellationToken = default)
    {
        var startUtc = DateTime.UtcNow.Date;
        var endUtc = startUtc.AddDays(1);
        var todaysSales = await _saleRepository.GetByDateRangeAsync(startUtc, endUtc, cancellationToken);
        return $"S-{startUtc:yyyyMMdd}-{(todaysSales.Count + 1):D4}";
    }

    public async Task<CompletedSaleResult> CompleteSaleAsync(
        CompleteSaleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Lines.Count == 0)
        {
            throw new InvalidOperationException("Cannot complete a sale with no items.");
        }

        var saleNumber = await GenerateNextSaleNumberAsync(cancellationToken);
        var totalLineDiscount = request.Lines.Sum(x => x.LineDiscount);
        var sale = new Sale
        {
            SaleNumber = saleNumber,
            CashierId = request.CashierId,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            DiscountAmount = request.OrderDiscountAmount + totalLineDiscount,
            TotalAmount = request.TotalAmount,
            AmountPaid = request.AmountPaid,
            Change = request.Change,
            PaymentMethod = request.PaymentMethod,
            Status = SaleStatus.Completed
        };

        foreach (var line in request.Lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId, cancellationToken)
                ?? throw new InvalidOperationException($"Product {line.ProductId} was not found.");

            if (product.StockQuantity < line.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for {product.Name}.");
            }

            sale.Items.Add(new SaleItem
            {
                ProductId = line.ProductId,
                ProductName = line.ProductName,
                ProductSku = line.ProductSku,
                UnitPrice = line.UnitPrice,
                TaxRate = line.TaxRate,
                Quantity = line.Quantity,
                Discount = line.LineDiscount,
                LineTotal = line.LineTotal
            });

            var quantityBefore = product.StockQuantity;
            product.StockQuantity -= line.Quantity;
            await _productRepository.UpdateAsync(product, cancellationToken);

            await _inventoryMovementRepository.AddAsync(new InventoryMovement
            {
                ProductId = product.Id,
                MovementType = InventoryMovementType.Sale,
                QuantityBefore = quantityBefore,
                QuantityChange = -line.Quantity,
                QuantityAfter = product.StockQuantity,
                Reference = saleNumber,
                UserId = request.CashierId
            }, cancellationToken);
        }

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _saleRepository.SaveChangesAsync(cancellationToken);

        var cashier = await _userRepository.GetByIdAsync(request.CashierId, cancellationToken);
        var storeName = await GetSettingAsync("Store.Name", "NovaPOS Store", cancellationToken);
        var footer = await GetSettingAsync("Receipt.Footer", "Thank you for your business!", cancellationToken);

        var receiptData = new SaleReceiptData
        {
            StoreName = storeName,
            FooterMessage = footer,
            SaleNumber = saleNumber,
            SaleDateUtc = sale.CreatedAt,
            CashierName = cashier?.FullName ?? "Cashier",
            PaymentMethod = request.PaymentMethod,
            SubTotal = request.SubTotal,
            TaxAmount = request.TaxAmount,
            DiscountAmount = request.OrderDiscountAmount + request.Lines.Sum(x => x.LineDiscount),
            TotalAmount = request.TotalAmount,
            AmountPaid = request.AmountPaid,
            Change = request.Change,
            ShowWatermark = request.ShowReceiptWatermark,
            Lines = request.Lines.Select(x => new SaleReceiptLine
            {
                Name = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                Discount = x.LineDiscount,
                LineTotal = x.LineTotal
            }).ToList()
        };

        var pdfBytes = _receiptService.GeneratePdf(receiptData);
        var pdfPath = await _receiptService.SavePdfAsync(receiptData, saleNumber, cancellationToken);

        if (_receiptService.IsAutoPrintEnabled())
        {
            await _receiptService.PrintAsync(pdfPath, cancellationToken);
        }

        await _auditService.LogAsync(
            "Sale.Created",
            "Sale",
            sale.Id.ToString(),
            newValues: new { sale.SaleNumber, sale.TotalAmount, sale.PaymentMethod },
            userId: request.CashierId,
            cancellationToken: cancellationToken);

        _logger.LogInformation("Sale {SaleNumber} completed for {Total:C}.", saleNumber, sale.TotalAmount);

        return new CompletedSaleResult
        {
            Sale = sale,
            ReceiptPdfPath = pdfPath,
            ReceiptPdfBytes = pdfBytes
        };
    }

    private async Task<string> GetSettingAsync(string key, string fallback, CancellationToken cancellationToken)
    {
        var setting = await _appSettingRepository.GetByKeyAsync(key, cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.Value) ? fallback : setting.Value;
    }
}
