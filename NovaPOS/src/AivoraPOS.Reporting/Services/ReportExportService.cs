using System.Globalization;
using System.Text;
using AivoraPOS.Core.Interfaces.Services;
using AivoraPOS.Core.Models.Reports;
using AivoraPOS.Reporting.Export;
using QuestPDF.Fluent;

namespace AivoraPOS.Reporting.Services;

public sealed class ReportExportService : IReportExportService
{
    public Task ExportDailySummaryPdfAsync(DailySummaryDto report, string storeName, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = ReportPdfDocumentFactory.CreateDailySummary(report, storeName);
        return WritePdfAsync(bytes, filePath, cancellationToken);
    }

    public Task ExportDateRangePdfAsync(DateRangeReportDto report, string storeName, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = ReportPdfDocumentFactory.CreateDateRangeReport(report, storeName);
        return WritePdfAsync(bytes, filePath, cancellationToken);
    }

    public Task ExportProductPerformancePdfAsync(IReadOnlyList<ProductPerformanceDto> report, DateTime from, DateTime to, string storeName, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = ReportPdfDocumentFactory.CreateProductPerformance(report, from, to, storeName);
        return WritePdfAsync(bytes, filePath, cancellationToken);
    }

    public Task ExportInventoryPdfAsync(IReadOnlyList<InventoryStatusDto> report, string storeName, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = ReportPdfDocumentFactory.CreateInventoryReport(report, storeName);
        return WritePdfAsync(bytes, filePath, cancellationToken);
    }

    public Task ExportCashierReportPdfAsync(IReadOnlyList<CashierReportDto> report, DateTime from, DateTime to, string storeName, string filePath, CancellationToken cancellationToken = default)
    {
        var bytes = ReportPdfDocumentFactory.CreateCashierReport(report, from, to, storeName);
        return WritePdfAsync(bytes, filePath, cancellationToken);
    }

    public async Task ExportInventoryCsvAsync(IReadOnlyList<InventoryStatusDto> report, string filePath, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Product,SKU,Category,Stock,Threshold,Status");

        foreach (var item in report)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(item.ProductName),
                EscapeCsv(item.Sku),
                EscapeCsv(item.CategoryName),
                item.StockQuantity.ToString(CultureInfo.InvariantCulture),
                item.LowStockThreshold.ToString(CultureInfo.InvariantCulture),
                item.Status.ToString()));
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), cancellationToken);
    }

    private static string EscapeCsv(string value) =>
        value.Contains('"') || value.Contains(',')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static async Task WritePdfAsync(byte[] bytes, string filePath, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(filePath, bytes, cancellationToken);
    }
}
