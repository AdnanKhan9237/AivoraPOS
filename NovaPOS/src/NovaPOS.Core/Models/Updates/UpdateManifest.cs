namespace NovaPOS.Core.Models.Updates;

public sealed class UpdateManifest
{
    public string Version { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string ReleaseNotes { get; set; } = string.Empty;
}

public sealed class UpdateCheckResult
{
    public bool IsUpdateAvailable { get; init; }

    public UpdateManifest? Manifest { get; init; }

    public static UpdateCheckResult None { get; } = new() { IsUpdateAvailable = false };
}
