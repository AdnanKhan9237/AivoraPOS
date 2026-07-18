using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NovaPOS.Reporting.Receipts;

public static class ReceiptDocumentFactory
{
    static ReceiptDocumentFactory()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] CreateSampleReceipt(string storeName, decimal total)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(20);
                page.Size(226, 800);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text(storeName).SemiBold().FontSize(14);
                page.Content().Column(column =>
                {
                    column.Spacing(4);
                    column.Item().Text($"Date: {DateTime.Now:g}");
                    column.Item().Text($"Total: {total:C}");
                    column.Item().Text("Thank you for your purchase!");
                });
            });
        }).GeneratePdf();
    }
}
