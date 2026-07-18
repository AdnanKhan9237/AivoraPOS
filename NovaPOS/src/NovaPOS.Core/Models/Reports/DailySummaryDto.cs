namespace NovaPOS.Core.Models.Reports;

public sealed class DailySummaryDto
{
    public DateTime ReportDate { get; init; }
    public decimal TotalRevenue { get; init; }
    public int TotalTransactions { get; init; }
    public decimal AverageTransactionValue { get; init; }
    public decimal CashRevenue { get; init; }
    public decimal CardRevenue { get; init; }
    public decimal MixedRevenue { get; init; }
    public IReadOnlyList<ChartDataPoint> HourlySales { get; init; } = Array.Empty<ChartDataPoint>();
    public IReadOnlyList<TopProductDto> TopProducts { get; init; } = Array.Empty<TopProductDto>();
    public IReadOnlyList<CashierPerformanceDto> CashierPerformance { get; init; } = Array.Empty<CashierPerformanceDto>();
}

public sealed class TopProductDto
{
    public string ProductName { get; init; } = string.Empty;
    public int UnitsSold { get; init; }
    public decimal Revenue { get; init; }
}

public sealed class CashierPerformanceDto
{
    public string CashierName { get; init; } = string.Empty;
    public int TransactionCount { get; init; }
    public decimal TotalRevenue { get; init; }
}
