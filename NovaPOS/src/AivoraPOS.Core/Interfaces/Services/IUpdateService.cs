using AivoraPOS.Core.Models.Updates;

namespace AivoraPOS.Core.Interfaces.Services;

public interface IUpdateService
{
    Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    Task DownloadAndInstallAsync(UpdateManifest manifest, CancellationToken cancellationToken = default);

    void DismissUpdate(string version);

    bool IsDismissed(string version);
}
