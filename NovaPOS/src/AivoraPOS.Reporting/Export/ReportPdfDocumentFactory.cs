using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Models.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AivoraPOS.Reporting.Export;

public static class ReportPdfDocumentFactory
{
    private static readonly string PrimaryColor = "#1F4E79";

    static ReportPdfDocumentFactory()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] CreateDailySummary(DailySummaryDto report, string storeName) =>
        CreateDocument(storeName, "Daily Sales Summary", report.ReportDate.ToString("D"), report.ReportDate, report.ReportDate, container =>
        {
            container.Item().Row(row =>
            {
                row.RelativeItem().Element(c => MetricCard(c, "Revenue", report.TotalRevenue.ToString("C")));
                row.RelativeItem().Element(c => MetricCard(c, "Transactions", report.TotalTransactions.ToString()));
                row.RelativeItem().Element(c => MetricCard(c, "Avg. Ticket", report.AverageTransactionValue.ToString("C")));
            });

            container.Item().PaddingTop(12).Text("Payment Methods").SemiBold();
            container.Item().Text($"Cash: {report.CashRevenue:C}  |  Card: {report.CardRevenue:C}  |  Mixed: {report.MixedRevenue:C}");

            container.Item().PaddingTop(12).Text("Top Products").SemiBold();
            foreach (var product in report.TopProducts)
            {
                container.Item().Text($"{product.ProductName} — {product.UnitsSold} units, {product.Revenue:C}");
            }

            container.Item().PaddingTop(12).Text("Cashier Performance").SemiBold();
            foreach (var cashier in report.CashierPerformance)
            {
                container.Item().Text($"{cashier.CashierName}: {cashier.TransactionCount} sales, {cashier.TotalRevenue:C}");
            }
        });

    public static byte[] CreateDateRangeReport(DateRangeReportDto report, string storeName) =>
        CreateDocument(storeName, "Date Range Report", $"{report.FromDate:d} – {report.ToDate:d}", report.FromDate, report.ToDate, container =>
        {
            container.Item().Row(row =>
            {
                row.RelativeItem().Element(c => MetricCard(c, "Revenue", report.TotalRevenue.ToString("C")));
                row.RelativeItem().Element(c => MetricCard(c, "Transactions", report.TotalTransactions.ToString()));
                row.RelativeItem().Element(c => MetricCard(c, "Items Sold", report.TotalItemsSold.ToString()));
            });

            container.Item().PaddingTop(12).Text("Revenue by Day").SemiBold();
            foreach (var point in report.RevenueTrend)
            {
                container.Item().Text($"{point.Label}: {point.Value:C}");
            }

            container.Item().PaddingTop(12).Text("Best Sellers").SemiBold();
            foreach (var product in report.BestSellingProducts.Take(15))
            {
                container.Item().Text($"{product.ProductName}: {product.UnitsSold} units, {product.Revenue:C}");
            }
        });

    public static byte[] CreateProductPerformance(IReadOnlyList<ProductPerformanceDto> report, DateTime from, DateTime to, string storeName) =>
        CreateDocument(storeName, "Product Performance", $"{from:d} – {to:d}", from, to, container =>
        {
            container.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Product").SemiBold();
                    header.Cell().Text("Units").SemiBold();
                    header.Cell().Text("Revenue").SemiBold();
                    header.Cell().Text("Profit").SemiBold();
                    header.Cell().Text("Margin %").SemiBold();
                });

                foreach (var item in report)
                {
                    table.Cell().Text(item.ProductName);
                    table.Cell().Text(item.UnitsSold.ToString());
                    table.Cell().Text(item.Revenue.ToString("C"));
                    table.Cell().Text(item.Profit.ToString("C"));
                    table.Cell().Text($"{item.ProfitMarginPercent:N1}%");
                }
            });
        });

    public static byte[] CreateInventoryReport(IReadOnlyList<InventoryStatusDto> report, string storeName) =>
        CreateDocument(storeName, "Inventory Report", DateTime.Today.ToString("D"), DateTime.Today, DateTime.Today, container =>
        {
            container.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Product").SemiBold();
                    header.Cell().Text("SKU").SemiBold();
                    header.Cell().Text("Stock").SemiBold();
                    header.Cell().Text("Threshold").SemiBold();
                    header.Cell().Text("Status").SemiBold();
                });

                foreach (var item in report)
                {
                    table.Cell().Text(item.ProductName);
                    table.Cell().Text(item.Sku);
                    table.Cell().Text(item.StockQuantity.ToString());
                    table.Cell().Text(item.LowStockThreshold.ToString());
                    table.Cell().Text(item.Status.ToString());
                }
            });
        });

    public static byte[] CreateCashierReport(IReadOnlyList<CashierReportDto> report, DateTime from, DateTime to, string storeName) =>
        CreateDocument(storeName, "Cashier Report", $"{from:d} – {to:d}", from, to, container =>
        {
            container.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Cashier").SemiBold();
                    header.Cell().Text("Sales").SemiBold();
                    header.Cell().Text("Transactions").SemiBold();
                    header.Cell().Text("Refunds").SemiBold();
                    header.Cell().Text("Refund $").SemiBold();
                });

                foreach (var item in report)
                {
                    table.Cell().Text(item.CashierName);
                    table.Cell().Text(item.TotalSales.ToString("C"));
                    table.Cell().Text(item.TotalTransactions.ToString());
                    table.Cell().Text(item.RefundsIssued.ToString());
                    table.Cell().Text(item.RefundAmount.ToString("C"));
                }
            });
        });

    private static byte[] CreateDocument(
        string storeName,
        string title,
        string dateRangeText,
        DateTime from,
        DateTime to,
        Action<ColumnDescriptor> buildContent)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().AlignLeft().Column(left =>
                        {
                            left.Item().Text(storeName).FontSize(16).SemiBold().FontColor(PrimaryColor);
                            left.Item().Text(title).FontSize(12).SemiBold();
                        });

                        row.ConstantItem(180).AlignRight().Column(right =>
                        {
                            right.Item().Text(ProductInfo.PdfGeneratedBy).FontSize(8).FontColor(Colors.Grey.Darken1);
                            right.Item().Text(ProductInfo.CompanyName).FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    });

                    column.Item().PaddingTop(8).LineHorizontal(1).LineColor(PrimaryColor);
                });

                page.Content().PaddingVertical(16).Column(buildContent);

                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"{title} — {dateRangeText}").FontSize(8).FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span($"{ProductInfo.PdfFooterRight()}  |  Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        }).GeneratePdf();
    }

    private static void MetricCard(IContainer container, string label, string value)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(column =>
        {
            column.Item().Text(label).FontSize(8).FontColor(Colors.Grey.Darken1);
            column.Item().Text(value).FontSize(14).SemiBold().FontColor(PrimaryColor);
        });
    }
}
