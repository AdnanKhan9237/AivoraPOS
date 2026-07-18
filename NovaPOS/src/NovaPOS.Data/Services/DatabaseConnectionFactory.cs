using Microsoft.Data.Sqlite;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.Data.Services;

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private static readonly string DatabaseKeyFilePath =
        Path.Combine(AppPaths.DataDirectory, "dbconfig.enc");

    private readonly IEncryptionService _encryptionService;

    public DatabaseConnectionFactory(IEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string GetConnectionString()
    {
        AppPaths.EnsureDirectoriesExist();

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = AppPaths.DatabaseFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Password = ResolveDatabasePassword()
        };

        return builder.ConnectionString;
    }

    private string ResolveDatabasePassword()
    {
        if (File.Exists(DatabaseKeyFilePath))
        {
            var encryptedPassword = File.ReadAllText(DatabaseKeyFilePath);
            return _encryptionService.Decrypt(encryptedPassword);
        }

        var password = _encryptionService.GenerateKey();
        var encrypted = _encryptionService.Encrypt(password);
        File.WriteAllText(DatabaseKeyFilePath, encrypted);
        return password;
    }
}
