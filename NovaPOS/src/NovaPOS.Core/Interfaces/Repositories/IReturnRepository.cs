using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IReturnRepository : IRepository<Return>
{
    Task<Return?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Return>> GetBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
}
