using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Models.Sales;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AivoraPOS.Reporting.Receipts;

public static class ReceiptDocumentFactory
{
    static ReceiptDocumentFactory()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] CreateSampleReceipt(string storeName, decimal total) =>
        CreateSaleReceipt(new SaleReceiptData
        {
            StoreName = storeName,
            SaleNumber = "SAMPLE",
            SaleDateUtc = DateTime.UtcNow,
            CashierName = "Demo",
            SubTotal = total,
            TaxAmount = 0,
            TotalAmount = total,
            AmountPaid = total,
            Lines =
            [
                new SaleReceiptLine { Name = "Sample Item", Quantity = 1, UnitPrice = total, LineTotal = total }
            ]
        });

    public static byte[] CreateSaleReceipt(SaleReceiptData data)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(16);
                page.Size(226, 800);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().AlignCenter().Text(data.StoreName).SemiBold().FontSize(14);
                    column.Item().AlignCenter().Text($"Receipt #{data.SaleNumber}").FontSize(9);
                    column.Item().AlignCenter().Text(data.SaleDateUtc.ToLocalTime().ToString("g")).FontSize(8);
                    column.Item().AlignCenter().Text($"Cashier: {data.CashierName}").FontSize(8);
                });

                page.Content().PaddingVertical(8).Column(column =>
                {
                    column.Spacing(4);

                    foreach (var line in data.Lines)
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"{line.Name} x{line.Quantity}");
                            row.ConstantItem(60).AlignRight().Text(line.LineTotal.ToString("C"));
                        });

                        if (line.Discount > 0)
                        {
                            column.Item().Text($"  Discount: -{line.Discount:C}").FontSize(8).Italic();
                        }
                    }

                    column.Item().PaddingTop(6).LineHorizontal(1);

                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal");
                        r.ConstantItem(60).AlignRight().Text(data.SubTotal.ToString("C"));
                    });
                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Tax");
                        r.ConstantItem(60).AlignRight().Text(data.TaxAmount.ToString("C"));
                    });

                    if (data.DiscountAmount > 0)
                    {
                        column.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Discount");
                            r.ConstantItem(60).AlignRight().Text($"-{data.DiscountAmount:C}");
                        });
                    }

                    column.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL").SemiBold();
                        r.ConstantItem(60).AlignRight().Text(data.TotalAmount.ToString("C")).SemiBold();
                    });

                    column.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text("Paid");
                        r.ConstantItem(60).AlignRight().Text(data.AmountPaid.ToString("C"));
                    });

                    if (data.Change > 0)
                    {
                        column.Item().Row(r =>
                        {
                            r.RelativeItem().Text("CHANGE").SemiBold().FontSize(11);
                            r.ConstantItem(60).AlignRight().Text(data.Change.ToString("C")).SemiBold().FontSize(11);
                        });
                    }

                    column.Item().PaddingTop(4).Text($"Payment: {data.PaymentMethod}").FontSize(8);

                    if (!string.IsNullOrWhiteSpace(data.FooterMessage))
                    {
                        column.Item().PaddingTop(8).AlignCenter().Text(data.FooterMessage).FontSize(8);
                    }

                    column.Item().PaddingTop(6).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    column.Item().PaddingTop(4).AlignCenter()
                        .Text(ProductInfo.ReceiptAttribution)
                        .FontSize(7)
                        .FontColor(Colors.Grey.Medium);

                    if (data.ShowWatermark)
                    {
                        column.Item().PaddingTop(12).AlignCenter().Text("TRIAL RECEIPT").FontSize(10).Italic();
                    }
                });
            });
        }).GeneratePdf();
    }
}
