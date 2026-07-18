using NovaPOS.Core.Models.Sales;

namespace NovaPOS.Core.Interfaces.Services;

public interface IReceiptService
{
    byte[] GeneratePdf(SaleReceiptData data);
    Task<string> SavePdfAsync(SaleReceiptData data, string saleNumber, CancellationToken cancellationToken = default);
    Task PrintAsync(string pdfPath, CancellationToken cancellationToken = default);
    bool IsAutoPrintEnabled();
}
