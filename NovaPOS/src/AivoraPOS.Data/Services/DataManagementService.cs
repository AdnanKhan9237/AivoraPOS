using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.Data.Services;

public sealed class DataManagementService : IDataManagementService
{
    private readonly IProductRepository _productRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<DataManagementService> _logger;

    public DataManagementService(
        IProductRepository productRepository,
        ISaleRepository saleRepository,
        IAuditLogRepository auditLogRepository,
        IAuditService auditService,
        ILogger<DataManagementService> logger)
    {
        _productRepository = productRepository;
        _saleRepository = saleRepository;
        _auditLogRepository = auditLogRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task BackupDatabaseAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        AppPaths.EnsureDirectoriesExist();
        if (!File.Exists(AppPaths.DatabaseFilePath))
        {
            throw new InvalidOperationException("Database file was not found.");
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(AppPaths.DatabaseFilePath, destinationPath, overwrite: true);
        await _auditService.LogAsync("DatabaseBackup", entityType: "Database", newValues: new { Path = destinationPath }, cancellationToken: cancellationToken);
        _logger.LogInformation("Database backed up to {Path}.", destinationPath);
    }

    public async Task RestoreDatabaseAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Backup file was not found.", sourcePath);
        }

        AppPaths.EnsureDirectoriesExist();
        File.Copy(sourcePath, AppPaths.DatabaseFilePath, overwrite: true);
        await _auditService.LogAsync("DatabaseRestore", entityType: "Database", newValues: new { Path = sourcePath }, cancellationToken: cancellationToken);
        _logger.LogWarning("Database restored from {Path}. Restart required.", sourcePath);
    }

    public async Task<string> ExportAllDataCsvAsync(string destinationPath, CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        var startUtc = DateTime.UtcNow.AddYears(-5);
        var sales = await _saleRepository.GetByDateRangeAsync(startUtc, DateTime.UtcNow, cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Type,Name,Sku,Quantity,Amount,Date");
        foreach (var product in products)
        {
            sb.AppendLine(string.Join(',',
                "Product",
                Escape(product.Name),
                Escape(product.Sku),
                product.StockQuantity.ToString(CultureInfo.InvariantCulture),
                product.SalePrice.ToString(CultureInfo.InvariantCulture),
                string.Empty));
        }

        foreach (var sale in sales)
        {
            sb.AppendLine(string.Join(',',
                "Sale",
                Escape(sale.SaleNumber),
                string.Empty,
                sale.Items.Count.ToString(CultureInfo.InvariantCulture),
                sale.TotalAmount.ToString(CultureInfo.InvariantCulture),
                sale.CreatedAt.ToString("O", CultureInfo.InvariantCulture)));
        }

        var directory = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(destinationPath, sb.ToString(), Encoding.UTF8, cancellationToken);
        await _auditService.LogAsync("DataExportCsv", entityType: "Database", newValues: new { Path = destinationPath }, cancellationToken: cancellationToken);
        return destinationPath;
    }

    public async Task<int> ClearAuditLogsOlderThanAsync(int months, CancellationToken cancellationToken = default)
    {
        if (months < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(months), "Months must be at least 1.");
        }

        var cutoff = DateTime.UtcNow.AddMonths(-months);
        var deleted = await _auditLogRepository.DeleteOlderThanAsync(cutoff, cancellationToken);
        await _auditService.LogAsync(
            "AuditLogsCleared",
            entityType: "AuditLog",
            newValues: new { OlderThanMonths = months, DeletedCount = deleted },
            cancellationToken: cancellationToken);
        return deleted;
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
