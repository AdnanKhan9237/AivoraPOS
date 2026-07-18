namespace NovaPOS.Core.Models.Reports;

public sealed class ProductPerformanceDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public int UnitsSold { get; init; }
    public decimal Revenue { get; init; }
    public decimal Cost { get; init; }
    public decimal Profit { get; init; }
    public decimal ProfitMarginPercent { get; init; }
}
