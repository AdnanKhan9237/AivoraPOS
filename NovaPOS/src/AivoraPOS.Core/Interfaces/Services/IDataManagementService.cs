namespace AivoraPOS.Core.Interfaces.Services;

public interface IDataManagementService
{
    Task BackupDatabaseAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task RestoreDatabaseAsync(string sourcePath, CancellationToken cancellationToken = default);
    Task<string> ExportAllDataCsvAsync(string destinationPath, CancellationToken cancellationToken = default);
    Task<int> ClearAuditLogsOlderThanAsync(int months, CancellationToken cancellationToken = default);
}
