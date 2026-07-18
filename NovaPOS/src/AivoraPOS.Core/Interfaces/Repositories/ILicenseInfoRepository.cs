using AivoraPOS.Core.Entities;

namespace AivoraPOS.Core.Interfaces.Repositories;

public interface ILicenseInfoRepository
{
    Task<LicenseInfo?> GetAsync(CancellationToken cancellationToken = default);
    Task<LicenseInfo> SaveAsync(LicenseInfo licenseInfo, CancellationToken cancellationToken = default);
    Task DeleteAsync(CancellationToken cancellationToken = default);
}
