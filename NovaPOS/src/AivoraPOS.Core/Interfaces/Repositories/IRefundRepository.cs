using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Repositories;

public interface IRefundRepository : IRepository<Refund>
{
    Task<Refund?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Refund>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
}
