using Microsoft.EntityFrameworkCore;
using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;

namespace AivoraPOS.Data.Repositories;

public class AppSettingRepository : IAppSettingRepository
{
    private readonly AppDbContext _context;

    public AppSettingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<AppSetting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.AppSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync(cancellationToken);
    }

    public async Task<AppSetting> AddAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        await _context.AppSettings.AddAsync(setting, cancellationToken);
        return setting;
    }

    public Task UpdateAsync(AppSetting setting, CancellationToken cancellationToken = default)
    {
        _context.AppSettings.Update(setting);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
