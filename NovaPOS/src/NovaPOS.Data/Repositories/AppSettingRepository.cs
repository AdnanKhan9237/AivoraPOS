using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class AppSettingRepository : Repository<AppSetting>, IAppSettingRepository
{
    public AppSettingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<AppSetting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<AppSetting>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.Category == category)
            .OrderBy(x => x.Key)
            .ToListAsync(cancellationToken);
    }
}
