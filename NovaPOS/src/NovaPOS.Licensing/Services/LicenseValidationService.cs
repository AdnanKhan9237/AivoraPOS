using Microsoft.Extensions.Logging;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Repositories;

namespace NovaPOS.Licensing.Services;

public class LicenseValidationService : ILicenseValidationService
{
    private readonly ILicenseInfoRepository _licenseInfoRepository;
    private readonly IHardwareFingerprintService _hardwareFingerprintService;
    private readonly ILogger<LicenseValidationService> _logger;

    public LicenseValidationService(
        ILicenseInfoRepository licenseInfoRepository,
        IHardwareFingerprintService hardwareFingerprintService,
        ILogger<LicenseValidationService> logger)
    {
        _licenseInfoRepository = licenseInfoRepository;
        _hardwareFingerprintService = hardwareFingerprintService;
        _logger = logger;
    }

    public async Task<bool> IsLicenseValidAsync(CancellationToken cancellationToken = default)
    {
        var license = await _licenseInfoRepository.GetAsync(cancellationToken);
        if (license is null)
        {
            _logger.LogWarning("No license record found.");
            return false;
        }

        if (!license.IsValid || license.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("License expired or marked invalid.");
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
