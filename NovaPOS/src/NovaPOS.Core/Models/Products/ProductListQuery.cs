using NovaPOS.Core.Enums;

namespace NovaPOS.Core.Models.Products;

public sealed class ProductListQuery
{
    public string? SearchTerm { get; init; }
    public Guid? CategoryId { get; init; }
    public ProductStatusFilter Status { get; init; } = ProductStatusFilter.All;
    public StockLevelFilter StockLevel { get; init; } = StockLevelFilter.All;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
