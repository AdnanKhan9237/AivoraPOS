namespace AivoraPOS.Core.Interfaces.Services;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
