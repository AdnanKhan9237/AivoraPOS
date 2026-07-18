using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface IAppSettingRepository
{
    Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppSetting> AddAsync(AppSetting setting, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppSetting setting, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
