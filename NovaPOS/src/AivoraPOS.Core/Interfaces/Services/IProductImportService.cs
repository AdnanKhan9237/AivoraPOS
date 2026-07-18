using AivoraPOS.Core.Models.Products;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IProductImportService
{
    string GetTemplateCsv();
    Task<IReadOnlyList<ProductImportRow>> ParseAndValidateAsync(Stream csvStream, CancellationToken cancellationToken = default);
    Task<ProductImportResult> ImportAsync(IReadOnlyList<ProductImportRow> rows, Guid userId, CancellationToken cancellationToken = default);
}
