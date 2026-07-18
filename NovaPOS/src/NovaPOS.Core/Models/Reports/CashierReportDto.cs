namespace NovaPOS.Core.Models.Reports;

public sealed class CashierReportDto
{
    public Guid CashierId { get; init; }
    public string CashierName { get; init; } = string.Empty;
    public decimal TotalSales { get; init; }
    public int TotalTransactions { get; init; }
    public int RefundsIssued { get; init; }
    public decimal RefundAmount { get; init; }
}
