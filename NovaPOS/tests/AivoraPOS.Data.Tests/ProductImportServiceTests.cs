using System.Text;
using AivoraPOS.Core.Models.Products;
using AivoraPOS.Data.Services;

namespace AivoraPOS.Data.Tests;

public class ProductImportServiceTests
{
    [Fact]
    public async Task ParseAndValidateAsync_FlagsMissingRequiredFields()
    {
        var csv = "Name,SKU,Price,Category,Stock,Barcode\r\n, ,-1, ,-5,";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));

        var service = new ProductImportService(null!, null!, null!, null!, null!);
        var rows = await service.ParseAndValidateAsync(stream);

        Assert.Single(rows);
        Assert.False(rows[0].IsValid);
        Assert.Contains(rows[0].Errors, e => e.Contains("Name"));
        Assert.Contains(rows[0].Errors, e => e.Contains("SKU"));
        Assert.Contains(rows[0].Errors, e => e.Contains("Price"));
    }

    [Fact]
    public void GetTemplateCsv_IncludesRequiredHeaders()
    {
        var service = new ProductImportService(null!, null!, null!, null!, null!);
        var template = service.GetTemplateCsv();

        Assert.Contains("Name,SKU,Price", template);
    }
}

public class ProductListQueryTests
{
    [Fact]
    public void PagedResult_ComputesTotalPages()
    {
        var result = new Core.Models.PagedResult<string>
        {
            TotalCount = 101,
            Page = 1,
            PageSize = 50,
            Items = Enumerable.Repeat("x", 50).ToList()
        };

        Assert.Equal(3, result.TotalPages);
    }
}
