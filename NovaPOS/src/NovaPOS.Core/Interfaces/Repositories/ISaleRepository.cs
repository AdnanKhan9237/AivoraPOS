using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
    Task<Sale?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startUtc, DateTime endUtc, CancellationToken cancellationToken = default);
}
