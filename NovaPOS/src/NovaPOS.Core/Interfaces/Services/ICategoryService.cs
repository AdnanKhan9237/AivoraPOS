using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Category> CreateAsync(string name, CancellationToken cancellationToken = default);
    Task<Category> RenameAsync(Guid categoryId, string name, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<int> CountActiveProductsAsync(Guid categoryId, CancellationToken cancellationToken = default);
}
