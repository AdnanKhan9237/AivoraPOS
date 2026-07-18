using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using NovaPOS.Core;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Updates;

namespace NovaPOS.App.Services;

public sealed class UpdateService : IUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly string _stateFilePath;

    public UpdateService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        _stateFilePath = Path.Combine(AppPaths.AppDataRoot, "update-state.json");
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = await _httpClient.GetStreamAsync(ProductInfo.UpdateFeedUrl, cancellationToken);
            var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(stream, JsonOptions, cancellationToken);
            if (manifest is null || string.IsNullOrWhiteSpace(manifest.Version) || string.IsNullOrWhiteSpace(manifest.Url))
            {
                return UpdateCheckResult.None;
            }

            if (!AppVersion.IsNewer(manifest.Version, AppVersion.Current))
            {
                return UpdateCheckResult.None;
            }

            if (IsDismissed(manifest.Version))
            {
                return UpdateCheckResult.None;
            }

            return new UpdateCheckResult
            {
                IsUpdateAvailable = true,
                Manifest = manifest
            };
        }
        catch (HttpRequestException)
        {
            return UpdateCheckResult.None;
        }
        catch (TaskCanceledException)
        {
            return UpdateCheckResult.None;
        }
        catch (JsonException)
        {
            return UpdateCheckResult.None;
        }
    }

    public async Task DownloadAndInstallAsync(UpdateManifest manifest, CancellationToken cancellationToken = default)
    {
        var downloadsDirectory = Path.Combine(AppPaths.AppDataRoot, "downloads");
        Directory.CreateDirectory(downloadsDirectory);

        var fileName = Path.GetFileName(new Uri(manifest.Url).LocalPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"AivoraPOS-{manifest.Version}.msi";
        }

        var installerPath = Path.Combine(downloadsDirectory, fileName);
        await using (var stream = await _httpClient.GetStreamAsync(manifest.Url, cancellationToken))
        await using (var fileStream = File.Create(installerPath))
        {
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = installerPath,
            UseShellExecute = true
        });

        Application.Current.Shutdown(0);
    }

    public void DismissUpdate(string version)
    {
        var state = LoadState();
        state.DismissedVersions[version] = DateTime.UtcNow;
        SaveState(state);
    }

    public bool IsDismissed(string version)
    {
        var state = LoadState();
        if (!state.DismissedVersions.TryGetValue(version, out var dismissedAtUtc))
        {
            return false;
        }

        return DateTime.UtcNow - dismissedAtUtc < TimeSpan.FromDays(7);
    }

    private UpdateState LoadState()
    {
        try
        {
            if (!File.Exists(_stateFilePath))
            {
                return new UpdateState();
            }

            var json = File.ReadAllText(_stateFilePath);
            return JsonSerializer.Deserialize<UpdateState>(json) ?? new UpdateState();
        }
        catch (IOException)
        {
            return new UpdateState();
        }
        catch (JsonException)
        {
            return new UpdateState();
        }
    }

    private void SaveState(UpdateState state)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_stateFilePath)!);
            var json = JsonSerializer.Serialize(state, JsonOptions);
            File.WriteAllText(_stateFilePath, json);
        }
        catch (IOException)
        {
            // Best effort persistence.
        }
    }

    private sealed class UpdateState
    {
        public Dictionary<string, DateTime> DismissedVersions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
