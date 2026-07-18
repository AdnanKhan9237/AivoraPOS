using System.Diagnostics;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using AivoraPOS.Core.Interfaces.Security;

namespace AivoraPOS.Security.Integrity;

public sealed class AppIntegrityService : IAppIntegrityService
{
    private readonly ILogger<AppIntegrityService> _logger;

    public AppIntegrityService(ILogger<AppIntegrityService> logger)
    {
        _logger = logger;
    }

    public Task<AppIntegrityResult> VerifyAsync(CancellationToken cancellationToken = default)
    {
        var isVm = DetectVirtualMachine();
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;

        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            return Task.FromResult(new AppIntegrityResult
            {
                IsValid = true,
                IsRunningInVirtualMachine = isVm,
                Message = "Integrity check skipped (development environment)."
            });
        }

        var checksumPath = Path.ChangeExtension(exePath, ".sha256");
        if (!File.Exists(checksumPath))
        {
            _logger.LogDebug("No checksum file found for integrity verification.");
            return Task.FromResult(new AppIntegrityResult
            {
                IsValid = true,
                IsRunningInVirtualMachine = isVm,
                Message = isVm ? "Running in a virtual machine." : "Integrity baseline not configured."
            });
        }

        var expected = File.ReadAllText(checksumPath).Trim();
        var actual = ComputeSha256(exePath);
        var isValid = string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            _logger.LogWarning("Application integrity check failed.");
        }

        return Task.FromResult(new AppIntegrityResult
        {
            IsValid = isValid,
            IsRunningInVirtualMachine = isVm,
            Message = isValid
                ? (isVm ? "Application verified. Virtual machine detected." : "Application verified.")
                : "Application files may have been modified."
        });
    }

    private static bool DetectVirtualMachine()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher("SELECT Model FROM Win32_ComputerSystem");
            foreach (var item in searcher.Get())
            {
                var model = item["Model"]?.ToString() ?? string.Empty;
                return model.Contains("Virtual", StringComparison.OrdinalIgnoreCase)
                       || model.Contains("VMware", StringComparison.OrdinalIgnoreCase)
                       || model.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string ComputeSha256(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
}
