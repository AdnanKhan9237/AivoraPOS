using NovaPOS.Core.Models.Reports;

namespace NovaPOS.Core.Interfaces.Services;

public interface IReportExportService
{
    Task ExportDailySummaryPdfAsync(DailySummaryDto report, string storeName, string filePath, CancellationToken cancellationToken = default);
    Task ExportDateRangePdfAsync(DateRangeReportDto report, string storeName, string filePath, CancellationToken cancellationToken = default);
    Task ExportProductPerformancePdfAsync(IReadOnlyList<ProductPerformanceDto> report, DateTime from, DateTime to, string storeName, string filePath, CancellationToken cancellationToken = default);
    Task ExportInventoryPdfAsync(IReadOnlyList<InventoryStatusDto> report, string storeName, string filePath, CancellationToken cancellationToken = default);
    Task ExportCashierReportPdfAsync(IReadOnlyList<CashierReportDto> report, DateTime from, DateTime to, string storeName, string filePath, CancellationToken cancellationToken = default);
    Task ExportInventoryCsvAsync(IReadOnlyList<InventoryStatusDto> report, string filePath, CancellationToken cancellationToken = default);
}
