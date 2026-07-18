using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Models.Sales;

public sealed class CompletedSaleResult
{
    public Sale Sale { get; init; } = null!;
    public string ReceiptPdfPath { get; init; } = string.Empty;
    public byte[] ReceiptPdfBytes { get; init; } = Array.Empty<byte>();
}
