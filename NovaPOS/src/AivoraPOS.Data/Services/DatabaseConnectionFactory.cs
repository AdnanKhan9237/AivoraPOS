using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Interfaces.Security;
using AivoraPOS.Core.Interfaces.Services;

namespace AivoraPOS.Data.Services;

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly IEncryptionService _encryptionService;

    public DatabaseConnectionFactory(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string GetConnectionString()
    {
        AppPaths.EnsureDirectoriesExist();

        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = AppPaths.DatabaseFilePath,
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
            Cache = Microsoft.Data.Sqlite.SqliteCacheMode.Shared,
            Password = _encryptionService.DeriveDatabasePassword()
        };

        return builder.ConnectionString;
    }
}
