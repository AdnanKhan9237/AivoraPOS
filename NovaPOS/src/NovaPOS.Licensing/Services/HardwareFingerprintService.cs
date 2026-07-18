using System.Management;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Interfaces.Licensing;

namespace NovaPOS.Licensing.Services;

public class HardwareFingerprintService : IHardwareFingerprintService
{
    private readonly ILogger<HardwareFingerprintService> _logger;
    private string? _cachedFingerprint;

    public HardwareFingerprintService(ILogger<HardwareFingerprintService> logger)
    {
        _logger = logger;
    }

    public string GetFingerprint()
    {
        if (!string.IsNullOrWhiteSpace(_cachedFingerprint))
        {
            return _cachedFingerprint;
        }

        var components = new List<string>
        {
            Environment.MachineName,
            Environment.UserName,
            Environment.OSVersion.VersionString
        };

        if (OperatingSystem.IsWindows())
        {
            TryAddWindowsIdentifiers(components);
        }

        var raw = string.Join("|", components);
        _cachedFingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        _logger.LogDebug("Hardware fingerprint generated.");
        return _cachedFingerprint;
    }

    [SupportedOSPlatform("windows")]
    private void TryAddWindowsIdentifiers(List<string> components)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (var item in searcher.Get().Cast<ManagementObject>())
            {
                components.Add(item["ProcessorId"]?.ToString() ?? string.Empty);
                break;
            }

            using var boardSearcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var item in boardSearcher.Get().Cast<ManagementObject>())
            {
                components.Add(item["SerialNumber"]?.ToString() ?? string.Empty);
                break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to read WMI hardware identifiers. Using fallback fingerprint.");
        }
    }
}
