namespace AivoraPOS.Core.Models.Products;

public sealed class ProductImportRow
{
    public int RowNumber { get; init; }
    public string? Name { get; init; }
    public string? Sku { get; init; }
    public string? Price { get; init; }
    public string? Category { get; init; }
    public string? Stock { get; init; }
    public string? Barcode { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public string ErrorsDisplay => Errors.Count == 0 ? string.Empty : string.Join("; ", Errors);
    public bool IsValid => Errors.Count == 0;
}
