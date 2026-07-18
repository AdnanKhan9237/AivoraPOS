using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class LicenseRecordRepository : Repository<LicenseRecord>, ILicenseRecordRepository
{
    public LicenseRecordRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<LicenseRecord?> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.ActivatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
