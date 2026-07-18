using Microsoft.Extensions.Logging;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Licensing.Services;

public class LicenseValidationService : ILicenseValidationService
{
    private readonly ILicenseRecordRepository _licenseRecordRepository;
    private readonly IHardwareFingerprintService _hardwareFingerprintService;
    private readonly ILogger<LicenseValidationService> _logger;

    public LicenseValidationService(
        ILicenseRecordRepository licenseRecordRepository,
        IHardwareFingerprintService hardwareFingerprintService,
        ILogger<LicenseValidationService> logger)
    {
        _licenseRecordRepository = licenseRecordRepository;
        _hardwareFingerprintService = hardwareFingerprintService;
        _logger = logger;
    }

    public async Task<bool> IsLicenseValidAsync(CancellationToken cancellationToken = default)
    {
        var license = await _licenseRecordRepository.GetActiveAsync(cancellationToken);
        if (license is null)
        {
            _logger.LogWarning("No active license record found.");
            return false;
        }

        if (!license.IsActive || license.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("License expired or inactive.");
            return false;
        }

        var fingerprint = _hardwareFingerprintService.GetFingerprint();
        if (!string.Equals(license.HardwareFingerprint, fingerprint, StringComparison.Ordinal))
        {
            _logger.LogWarning("License hardware fingerprint mismatch.");
            return false;
        }

        return true;
    }

    public Task<bool> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("License activation requested (implementation pending).");
        return Task.FromResult(false);
    }
}
