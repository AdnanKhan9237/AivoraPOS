using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Interfaces.Services;

namespace NovaPOS.Data.Services;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly AppDbContext _context;
    private readonly IDatabaseSeeder _databaseSeeder;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        AppDbContext context,
        IDatabaseSeeder databaseSeeder,
        ILogger<DatabaseInitializer> logger)
    {
        _context = context;
        _databaseSeeder = databaseSeeder;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Applying database migrations...");
            await _context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Running database seed...");
            await _databaseSeeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed.");
            throw;
        }
    }
}
