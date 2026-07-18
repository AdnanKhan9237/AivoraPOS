namespace NovaPOS.Core.Models.Reports;

public sealed class DateRangeReportDto
{
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public decimal TotalRevenue { get; init; }
    public int TotalTransactions { get; init; }
    public int TotalItemsSold { get; init; }
    public IReadOnlyList<ChartDataPoint> RevenueTrend { get; init; } = Array.Empty<ChartDataPoint>();
    public IReadOnlyList<TopProductDto> BestSellingProducts { get; init; } = Array.Empty<TopProductDto>();
    public IReadOnlyList<ChartDataPoint> CategoryPerformance { get; init; } = Array.Empty<ChartDataPoint>();
}
