using AivoraPOS.Core.Models.Reports;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IReportService
{
    Task<DailySummaryDto> GetDailySummaryAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<DateRangeReportDto> GetDateRangeReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<List<InventoryStatusDto>> GetInventoryStatusAsync(CancellationToken cancellationToken = default);
    Task<List<CashierReportDto>> GetCashierReportAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
