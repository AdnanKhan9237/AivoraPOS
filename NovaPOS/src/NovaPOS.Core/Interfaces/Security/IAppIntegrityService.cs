namespace NovaPOS.Core.Interfaces.Security;

public interface IAppIntegrityService
{
    Task<AppIntegrityResult> VerifyAsync(CancellationToken cancellationToken = default);
}

public sealed class AppIntegrityResult
{
    public bool IsValid { get; init; }
    public bool IsRunningInVirtualMachine { get; init; }
    public string Message { get; init; } = string.Empty;
}
