using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Reports;
using NovaPOS.Data;

namespace NovaPOS.Reporting.Services;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DailySummaryDto> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = GetLocalDayRangeUtc(date);

        var sales = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Cashier)
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc && s.Status == SaleStatus.Completed)
            .ToListAsync(cancellationToken);

        var saleIds = sales.Select(s => s.Id).ToList();

        var saleItems = saleIds.Count == 0
            ? new List<Core.Entities.SaleItem>()
            : await _context.SaleItems
                .AsNoTracking()
                .Where(i => saleIds.Contains(i.SaleId))
                .ToListAsync(cancellationToken);

        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var transactionCount = sales.Count;

        var hourlySales = Enumerable.Range(0, 24)
            .Select(hour =>
            {
                var hourRevenue = sales
                    .Where(s => s.CreatedAt.ToLocalTime().Hour == hour)
                    .Sum(s => s.TotalAmount);
                return new ChartDataPoint($"{hour:00}:00", hourRevenue);
            })
            .ToList();

        var topProducts = saleItems
            .GroupBy(i => i.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.UnitsSold)
            .Take(5)
            .ToList();

        var cashierPerformance = sales
            .GroupBy(s => s.CashierId)
            .Select(g => new CashierPerformanceDto
            {
                CashierName = g.First().Cashier?.FullName ?? "Unknown",
                TransactionCount = g.Count(),
                TotalRevenue = g.Sum(s => s.TotalAmount)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .ToList();

        return new DailySummaryDto
        {
            ReportDate = date.Date,
            TotalRevenue = totalRevenue,
            TotalTransactions = transactionCount,
            AverageTransactionValue = transactionCount == 0 ? 0 : Math.Round(totalRevenue / transactionCount, 2),
            CashRevenue = sales.Where(s => s.PaymentMethod == PaymentMethod.Cash).Sum(s => s.TotalAmount),
            CardRevenue = sales.Where(s => s.PaymentMethod == PaymentMethod.Card).Sum(s => s.TotalAmount),
            MixedRevenue = sales.Where(s => s.PaymentMethod == PaymentMethod.Mixed).Sum(s => s.TotalAmount),
            HourlySales = hourlySales,
            TopProducts = topProducts,
            CashierPerformance = cashierPerformance
        };
    }

    public async Task<DateRangeReportDto> GetDateRangeReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = GetLocalRangeUtc(from, to);

        var sales = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc && s.Status == SaleStatus.Completed)
            .ToListAsync(cancellationToken);

        var saleIds = sales.Select(s => s.Id).ToList();

        var saleItems = saleIds.Count == 0
            ? new List<Core.Entities.SaleItem>()
            : await _context.SaleItems
                .AsNoTracking()
                .Include(i => i.Product)
                .ThenInclude(p => p.Category)
                .Where(i => saleIds.Contains(i.SaleId))
                .ToListAsync(cancellationToken);

        var dayCount = Math.Max(1, (to.Date - from.Date).Days + 1);
        var revenueTrend = Enumerable.Range(0, dayCount)
            .Select(offset =>
            {
                var day = from.Date.AddDays(offset);
                var (dayStart, dayEnd) = GetLocalDayRangeUtc(day);
                var dayRevenue = sales
                    .Where(s => s.CreatedAt >= dayStart && s.CreatedAt < dayEnd)
                    .Sum(s => s.TotalAmount);
                return new ChartDataPoint(day.ToString("MM/dd"), dayRevenue);
            })
            .ToList();

        var bestSelling = saleItems
            .GroupBy(i => i.ProductName)
            .Select(g => new TopProductDto
            {
                ProductName = g.Key,
                UnitsSold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        var categoryPerformance = saleItems
            .GroupBy(i => i.Product?.Category?.Name ?? "Uncategorized")
            .Select(g => new ChartDataPoint(g.Key, g.Sum(x => x.LineTotal)))
            .OrderByDescending(x => x.Value)
            .ToList();

        return new DateRangeReportDto
        {
            FromDate = from.Date,
            ToDate = to.Date,
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalTransactions = sales.Count,
            TotalItemsSold = saleItems.Sum(i => i.Quantity),
            RevenueTrend = revenueTrend,
            BestSellingProducts = bestSelling,
            CategoryPerformance = categoryPerformance
        };
    }

    public async Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = GetLocalRangeUtc(from, to);

        var saleIds = await _context.Sales
            .AsNoTracking()
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc && s.Status == SaleStatus.Completed)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        if (saleIds.Count == 0)
        {
            return new List<ProductPerformanceDto>();
        }

        var items = await _context.SaleItems
            .AsNoTracking()
            .Include(i => i.Product)
            .Where(i => saleIds.Contains(i.SaleId))
            .ToListAsync(cancellationToken);

        return items
            .GroupBy(i => i.ProductId)
            .Select(g =>
            {
                var product = g.First().Product;
                var revenue = g.Sum(x => x.LineTotal);
                var units = g.Sum(x => x.Quantity);
                var cost = product is null ? 0 : product.PurchasePrice * units;
                var profit = revenue - cost;
                var margin = revenue == 0 ? 0 : Math.Round(profit / revenue * 100, 2);

                return new ProductPerformanceDto
                {
                    ProductId = g.Key,
                    ProductName = g.First().ProductName,
                    Sku = g.First().ProductSku,
                    UnitsSold = units,
                    Revenue = revenue,
                    Cost = cost,
                    Profit = profit,
                    ProfitMarginPercent = margin
                };
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    public async Task<List<InventoryStatusDto>> GetInventoryStatusAsync(CancellationToken cancellationToken = default)
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

        return products.Select(p => new InventoryStatusDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            Sku = p.Sku,
            CategoryName = p.Category?.Name ?? string.Empty,
            StockQuantity = p.StockQuantity,
            LowStockThreshold = p.LowStockThreshold,
            Status = p.StockQuantity <= 0
                ? InventoryStockStatus.OutOfStock
                : p.StockQuantity <= p.LowStockThreshold
                    ? InventoryStockStatus.LowStock
                    : InventoryStockStatus.Healthy
        }).ToList();
    }

    public async Task<List<CashierReportDto>> GetCashierReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
    {
        var (startUtc, endUtc) = GetLocalRangeUtc(from, to);

        var sales = await _context.Sales
            .AsNoTracking()
            .Include(s => s.Cashier)
            .Where(s => s.CreatedAt >= startUtc && s.CreatedAt < endUtc && s.Status == SaleStatus.Completed)
            .ToListAsync(cancellationToken);

        var refunds = await _context.Refunds
            .AsNoTracking()
            .Include(r => r.ProcessedBy)
            .Where(r => r.CreatedAt >= startUtc && r.CreatedAt < endUtc)
            .ToListAsync(cancellationToken);

        var cashierIds = sales.Select(s => s.CashierId)
            .Union(refunds.Select(r => r.ProcessedById))
            .Distinct();

        return cashierIds.Select(id =>
        {
            var cashierSales = sales.Where(s => s.CashierId == id).ToList();
            var cashierRefunds = refunds.Where(r => r.ProcessedById == id).ToList();
            var name = cashierSales.FirstOrDefault()?.Cashier?.FullName
                       ?? cashierRefunds.FirstOrDefault()?.ProcessedBy?.FullName
                       ?? "Unknown";

            return new CashierReportDto
            {
                CashierId = id,
                CashierName = name,
                TotalSales = cashierSales.Sum(s => s.TotalAmount),
                TotalTransactions = cashierSales.Count,
                RefundsIssued = cashierRefunds.Count,
                RefundAmount = cashierRefunds.Sum(r => r.RefundAmount)
            };
        })
        .OrderByDescending(x => x.TotalSales)
        .ToList();
    }

    private static (DateTime startUtc, DateTime endUtc) GetLocalDayRangeUtc(DateTime localDate)
    {
        var startLocal = localDate.Date;
        var endLocal = startLocal.AddDays(1);
        return (startLocal.ToUniversalTime(), endLocal.ToUniversalTime());
    }

    private static (DateTime startUtc, DateTime endUtc) GetLocalRangeUtc(DateTime from, DateTime to)
    {
        var startLocal = from.Date;
        var endLocal = to.Date.AddDays(1);
        return (startLocal.ToUniversalTime(), endLocal.ToUniversalTime());
    }
}
