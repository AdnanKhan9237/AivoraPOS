using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AivoraPOS.Core.Constants;

namespace AivoraPOS.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        AppPaths.EnsureDirectoriesExist();

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = AppPaths.DatabaseFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Password = "AivoraPOS-DesignTime-Key"
        };

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite(builder.ConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
