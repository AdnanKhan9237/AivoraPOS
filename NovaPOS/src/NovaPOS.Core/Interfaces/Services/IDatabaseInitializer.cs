namespace NovaPOS.Core.Interfaces.Services;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
