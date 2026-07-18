using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<IReadOnlyList<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
}
