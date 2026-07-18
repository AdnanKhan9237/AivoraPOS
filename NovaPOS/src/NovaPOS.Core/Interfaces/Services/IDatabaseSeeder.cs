namespace NovaPOS.Core.Interfaces.Services;

public interface IDatabaseSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
