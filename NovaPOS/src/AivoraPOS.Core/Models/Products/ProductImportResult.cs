namespace AivoraPOS.Core.Models.Products;

public sealed class ProductImportResult
{
    public int TotalRows { get; init; }
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
}
