using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface ILicenseInfoRepository
{
    Task<LicenseInfo?> GetAsync(CancellationToken cancellationToken = default);
    Task<LicenseInfo> SaveAsync(LicenseInfo licenseInfo, CancellationToken cancellationToken = default);
}
