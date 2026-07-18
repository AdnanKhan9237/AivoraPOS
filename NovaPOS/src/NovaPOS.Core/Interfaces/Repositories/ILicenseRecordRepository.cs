using NovaPOS.Core.Entities;

namespace NovaPOS.Core.Interfaces.Repositories;

public interface ILicenseRecordRepository : IRepository<LicenseRecord>
{
    Task<LicenseRecord?> GetActiveAsync(CancellationToken cancellationToken = default);
}
