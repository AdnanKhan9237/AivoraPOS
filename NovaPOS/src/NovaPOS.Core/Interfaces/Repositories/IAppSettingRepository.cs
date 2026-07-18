using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IAppSettingRepository : IRepository<AppSetting>
{
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
}
