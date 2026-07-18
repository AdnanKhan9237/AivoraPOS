using AivoraPOS.Core.Models.Reports;
using AivoraPOS.Reporting.Export;

namespace AivoraPOS.Reporting.Tests;

public class ReportPdfDocumentFactoryTests
{
    [Fact]
    public void CreateDailySummary_ReturnsValidPdf()
    {
        var bytes = ReportPdfDocumentFactory.CreateDailySummary(new DailySummaryDto
        {
            ReportDate = DateTime.Today,
            TotalRevenue = 1500m,
            TotalTransactions = 42,
            AverageTransactionValue = 35.71m,
            TopProducts = [new TopProductDto { ProductName = "Coffee", UnitsSold = 10, Revenue = 35m }]
        }, "Test Store");

        Assert.NotEmpty(bytes);
        Assert.Equal('%', (char)bytes[0]);
        Assert.Equal('P', (char)bytes[1]);
    }
}
