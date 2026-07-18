using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Products;

namespace NovaPOS.Data.Services;

public sealed class ProductImportService : IProductImportService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductService _productService;
    private readonly IInventoryAlertService _inventoryAlertService;
    private readonly ILogger<ProductImportService> _logger;

    public ProductImportService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProductService productService,
        IInventoryAlertService inventoryAlertService,
        ILogger<ProductImportService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productService = productService;
        _inventoryAlertService = inventoryAlertService;
        _logger = logger;
    }

    public string GetTemplateCsv() =>
        "Name,SKU,Price,Category,Stock,Barcode\r\n" +
        "Sample Product,SKU-001,9.99,Beverages,25,8412345678901\r\n";

    public async Task<IReadOnlyList<ProductImportRow>> ParseAndValidateAsync(Stream csvStream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(csvStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var lines = content
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        if (lines.Count == 0)
        {
            return [];
        }

        var header = SplitCsvLine(lines[0]);
        var columnMap = MapColumns(header);
        var rows = new List<ProductImportRow>();

        for (var i = 1; i < lines.Count; i++)
        {
            var values = SplitCsvLine(lines[i]);
            var row = new ProductImportRow
            {
                RowNumber = i + 1,
                Name = GetValue(values, columnMap, "name"),
                Sku = GetValue(values, columnMap, "sku"),
                Price = GetValue(values, columnMap, "price"),
                Category = GetValue(values, columnMap, "category"),
                Stock = GetValue(values, columnMap, "stock"),
                Barcode = GetValue(values, columnMap, "barcode")
            };

            var errors = ValidateRow(row);
            if (!string.IsNullOrWhiteSpace(row.Sku))
            {
                var isUnique = await _productRepository.IsSkuUniqueAsync(row.Sku, cancellationToken: cancellationToken);
                if (!isUnique)
                {
                    errors.Add($"SKU '{row.Sku}' already exists.");
                }
            }

            rows.Add(new ProductImportRow
            {
                RowNumber = row.RowNumber,
                Name = row.Name,
                Sku = row.Sku,
                Price = row.Price,
                Category = row.Category,
                Stock = row.Stock,
                Barcode = row.Barcode,
                Errors = errors
            });
        }

        return rows;
    }

    public async Task<ProductImportResult> ImportAsync(IReadOnlyList<ProductImportRow> rows, Guid userId, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var imported = 0;
        var defaultTaxRate = await _productService.GetDefaultTaxRateAsync(cancellationToken);
        var categories = (await _categoryRepository.GetAllOrderedAsync(cancellationToken)).ToList();

        foreach (var row in rows)
        {
            if (!row.IsValid)
            {
                errors.Add($"Row {row.RowNumber}: {string.Join("; ", row.Errors)}");
                continue;
            }

            try
            {
                var category = await ResolveCategoryAsync(row.Category, categories, cancellationToken);
                var salePrice = decimal.Parse(row.Price!, CultureInfo.InvariantCulture);
                var stock = string.IsNullOrWhiteSpace(row.Stock) ? 0 : int.Parse(row.Stock, CultureInfo.InvariantCulture);

                await _productService.SaveAsync(new ProductSaveRequest
                {
                    Name = row.Name!,
                    Sku = row.Sku!,
                    Barcode = row.Barcode,
                    CategoryId = category.Id,
                    PurchasePrice = salePrice * 0.6m,
                    SalePrice = salePrice,
                    TaxRate = defaultTaxRate,
                    StockQuantity = stock,
                    LowStockThreshold = 5,
                    IsActive = true,
                    UserId = userId
                }, cancellationToken);

                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row.RowNumber}: {ex.Message}");
                _logger.LogWarning(ex, "Failed to import row {RowNumber}.", row.RowNumber);
            }
        }

        await _inventoryAlertService.RefreshAsync(cancellationToken);

        return new ProductImportResult
        {
            TotalRows = rows.Count,
            ImportedCount = imported,
            SkippedCount = rows.Count - imported,
            Errors = errors
        };
    }

    private async Task<Category> ResolveCategoryAsync(string? categoryName, List<Category> categories, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            var fallback = categories.FirstOrDefault(x => x.IsActive)
                ?? throw new InvalidOperationException("No categories are available. Create a category first.");
            return fallback;
        }

        var existing = categories.FirstOrDefault(x =>
            x.Name.Equals(categoryName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            return existing;
        }

        var created = new Category
        {
            Name = categoryName.Trim(),
            IsActive = true
        };

        await _categoryRepository.AddAsync(created, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        categories.Add(created);
        return created;
    }

    private static List<string> ValidateRow(ProductImportRow row)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.Name))
        {
            errors.Add("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(row.Sku))
        {
            errors.Add("SKU is required.");
        }

        if (string.IsNullOrWhiteSpace(row.Price) || !decimal.TryParse(row.Price, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) || price <= 0)
        {
            errors.Add("Price must be a positive decimal.");
        }

        if (!string.IsNullOrWhiteSpace(row.Stock) && (!int.TryParse(row.Stock, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock) || stock < 0))
        {
            errors.Add("Stock must be a non-negative integer.");
        }

        return errors;
    }

    private static Dictionary<string, int> MapColumns(IReadOnlyList<string> header)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < header.Count; i++)
        {
            map[header[i].Trim()] = i;
        }

        return map;
    }

    private static string? GetValue(IReadOnlyList<string> values, IReadOnlyDictionary<string, int> map, string column)
    {
        if (!map.TryGetValue(column, out var index) || index >= values.Count)
        {
            return null;
        }

        var value = values[index].Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var values = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (c == ',' && !inQuotes)
            {
                values.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        values.Add(current.ToString());
        return values;
    }
}
