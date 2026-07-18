using System.Reflection;

namespace NovaPOS.Core;

public static class AppVersion
{
    public static string Current { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var informational = typeof(AppVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            var plusIndex = informational.IndexOf('+', StringComparison.Ordinal);
            return plusIndex >= 0 ? informational[..plusIndex] : informational;
        }

        var version = typeof(AppVersion).Assembly.GetName().Version;
        return version is null ? "1.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    public static bool IsNewer(string remoteVersion, string localVersion)
    {
        if (!TryParse(remoteVersion, out var remote))
        {
            return false;
        }

        if (!TryParse(localVersion, out var local))
        {
            return true;
        }

        return remote > local;
    }

    private static bool TryParse(string value, out Version version)
    {
        version = new Version(0, 0, 0);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Trim();
        if (normalized.StartsWith("v", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[1..];
        }

        var parts = normalized.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        try
        {
            var major = int.Parse(parts[0]);
            var minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            var build = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            version = new Version(major, minor, build);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }
    }
}
