namespace NovaPOS.Core.Interfaces.Licensing;

public interface ILicenseValidationService
{
    Task<bool> IsLicenseValidAsync(CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(string licenseKey, CancellationToken cancellationToken = default);
}
