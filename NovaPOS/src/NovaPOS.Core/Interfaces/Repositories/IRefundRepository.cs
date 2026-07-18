using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IRefundRepository : IRepository<Refund>
{
    Task<Refund?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Refund>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}
