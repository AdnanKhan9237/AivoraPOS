using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Updates;

namespace NovaPOS.App.Services;

public sealed class UpdateCoordinator
{
    private readonly IUpdateService _updateService;

    public UpdateCoordinator(IUpdateService updateService)
    {
        _updateService = updateService;
    }

    public UpdateManifest? AvailableUpdate { get; private set; }

    public bool HasUpdate => AvailableUpdate is not null;

    public string BannerText => AvailableUpdate is null
        ? string.Empty
        : $"Update available: v{AvailableUpdate.Version}";

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        var result = await _updateService.CheckForUpdatesAsync(cancellationToken);
        AvailableUpdate = result.IsUpdateAvailable ? result.Manifest : null;
    }

    public Task InstallAsync(CancellationToken cancellationToken = default)
    {
        if (AvailableUpdate is null)
        {
            return Task.CompletedTask;
        }

        return _updateService.DownloadAndInstallAsync(AvailableUpdate, cancellationToken);
    }

    public void Dismiss()
    {
        if (AvailableUpdate is null)
        {
            return;
        }

        _updateService.DismissUpdate(AvailableUpdate.Version);
        AvailableUpdate = null;
    }
}
