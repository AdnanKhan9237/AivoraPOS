using System.Diagnostics;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Sales;
using NovaPOS.Reporting.Receipts;
using QuestPDF.Fluent;

namespace NovaPOS.Reporting.Services;

public sealed class ReceiptService : IReceiptService
{
    private readonly IAppSettingRepository _appSettingRepository;

    public ReceiptService(IAppSettingRepository appSettingRepository)
    {
        _appSettingRepository = appSettingRepository;
    }

    public byte[] GeneratePdf(SaleReceiptData data) => ReceiptDocumentFactory.CreateSaleReceipt(data);

    public async Task<string> SavePdfAsync(SaleReceiptData data, string saleNumber, CancellationToken cancellationToken = default)
    {
        var pdfBytes = GeneratePdf(data);
        var saleDate = data.SaleDateUtc.ToLocalTime();
        var directory = Path.Combine(AppPaths.ReceiptsDirectory, saleDate.ToString("yyyy"), saleDate.ToString("MM"));
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{saleNumber}.pdf");
        await File.WriteAllBytesAsync(filePath, pdfBytes, cancellationToken);
        return filePath;
    }

    public async Task PrintAsync(string pdfPath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(pdfPath) || !OperatingSystem.IsWindows())
        {
            return;
        }

        var printerName = await GetPrinterNameAsync(cancellationToken);
        var startInfo = new ProcessStartInfo
        {
            FileName = pdfPath,
            Verb = string.IsNullOrWhiteSpace(printerName) ? "print" : "printto",
            CreateNoWindow = true,
            UseShellExecute = true
        };

        if (!string.IsNullOrWhiteSpace(printerName))
        {
            startInfo.Arguments = $"\"{printerName}\"";
        }

        Process.Start(startInfo);
    }

    public bool IsAutoPrintEnabled()
    {
        var setting = _appSettingRepository.GetByKeyAsync("Receipt.AutoPrint").GetAwaiter().GetResult();
        return bool.TryParse(setting?.Value, out var enabled) && enabled;
    }

    private async Task<string?> GetPrinterNameAsync(CancellationToken cancellationToken)
    {
        var setting = await _appSettingRepository.GetByKeyAsync("Printer.Name", cancellationToken);
        return string.IsNullOrWhiteSpace(setting?.Value) ? null : setting.Value;
    }
}
