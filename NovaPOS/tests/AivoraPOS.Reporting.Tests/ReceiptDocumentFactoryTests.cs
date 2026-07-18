using AivoraPOS.Core.Models.Sales;
using AivoraPOS.Reporting.Receipts;

namespace AivoraPOS.Reporting.Tests;

public class ReceiptDocumentFactoryTests
{
    [Fact]
    public void CreateSaleReceipt_ReturnsPdfBytes()
    {
        var bytes = ReceiptDocumentFactory.CreateSaleReceipt(new SaleReceiptData
        {
            StoreName = "Test Store",
            SaleNumber = "S-20260718-0001",
            SaleDateUtc = DateTime.UtcNow,
            CashierName = "Cashier",
            SubTotal = 10m,
            TaxAmount = 0.83m,
            TotalAmount = 10.83m,
            AmountPaid = 15m,
            Change = 4.17m,
            Lines =
            [
                new SaleReceiptLine { Name = "Coffee", Quantity = 2, UnitPrice = 5m, LineTotal = 10m }
            ]
        });

        Assert.NotEmpty(bytes);
        Assert.Equal(0x25, bytes[0]);
        Assert.Equal(0x50, bytes[1]);
    }
}
