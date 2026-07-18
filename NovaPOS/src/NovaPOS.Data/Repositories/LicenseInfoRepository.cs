using Microsoft.EntityFrameworkCore;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Data.Repositories;

public class LicenseInfoRepository : ILicenseInfoRepository
{
    private readonly AppDbContext _context;

    public LicenseInfoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LicenseInfo?> GetAsync(CancellationToken cancellationToken = default)
    {
        return await _context.LicenseInfos.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<LicenseInfo> SaveAsync(LicenseInfo licenseInfo, CancellationToken cancellationToken = default)
    {
        var existing = await _context.LicenseInfos.FirstOrDefaultAsync(cancellationToken);
        if (existing is null)
        {
            await _context.LicenseInfos.AddAsync(licenseInfo, cancellationToken);
        }
        else
        {
            existing.LicenseKey = licenseInfo.LicenseKey;
            existing.BusinessName = licenseInfo.BusinessName;
            existing.ActivatedAt = licenseInfo.ActivatedAt;
            existing.ExpiresAt = licenseInfo.ExpiresAt;
            existing.HardwareFingerprint = licenseInfo.HardwareFingerprint;
            existing.Plan = licenseInfo.Plan;
            existing.IsValid = licenseInfo.IsValid;
            _context.LicenseInfos.Update(existing);
            licenseInfo = existing;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return licenseInfo;
    }
}
