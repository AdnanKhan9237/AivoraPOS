using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace AivoraPOS.Licensing;

public static class HardwareFingerprint
{
    public sealed record FingerprintData(
        string Hash,
        string CpuId,
        string BoardSerial,
        string WindowsId);

    public static FingerprintData Generate()
    {
        var cpuId = ReadCpuId();
        var boardSerial = ReadBoardSerial();
        var windowsId = ReadWindowsInstallationId();

        var combined = $"{cpuId}|{boardSerial}|{windowsId}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        var hash = Convert.ToHexString(hashBytes)[..32];

        return new FingerprintData(hash, cpuId, boardSerial, windowsId);
    }

    public static bool MatchesWithTolerance(FingerprintData stored, FingerprintData current, int maxMismatches = 1)
    {
        var mismatches = 0;

        if (!string.Equals(stored.CpuId, current.CpuId, StringComparison.Ordinal))
        {
            mismatches++;
        }

        if (!string.Equals(stored.BoardSerial, current.BoardSerial, StringComparison.Ordinal))
        {
            mismatches++;
        }

        if (!string.Equals(stored.WindowsId, current.WindowsId, StringComparison.Ordinal))
        {
            mismatches++;
        }

        return mismatches <= maxMismatches;
    }

    public static string Serialize(FingerprintData data) =>
        $"{data.Hash}|{data.CpuId}|{data.BoardSerial}|{data.WindowsId}";

    public static FingerprintData Deserialize(string value)
    {
        var parts = value.Split('|', StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
        {
            throw new FormatException("Invalid hardware fingerprint format.");
        }

        return new FingerprintData(parts[0], parts[1], parts[2], parts[3]);
    }

    private static string ReadCpuId()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Environment.MachineName;
        }

        return ReadWmiValue("SELECT ProcessorId FROM Win32_Processor", "ProcessorId")
               ?? Environment.MachineName;
    }

    private static string ReadBoardSerial()
    {
        if (!OperatingSystem.IsWindows())
        {
            return "NON-WIN-BOARD";
        }

        return ReadWmiValue("SELECT SerialNumber FROM Win32_BaseBoard", "SerialNumber")
               ?? "UNKNOWN-BOARD";
    }

    private static string ReadWindowsInstallationId()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Environment.MachineName;
        }

        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var machineGuid = key?.GetValue("MachineGuid")?.ToString();
            if (!string.IsNullOrWhiteSpace(machineGuid))
            {
                return machineGuid;
            }
        }
        catch
        {
            // Fall through to WMI fallback.
        }

        return ReadWmiValue("SELECT SerialNumber FROM Win32_OperatingSystem", "SerialNumber")
               ?? Environment.MachineName;
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadWmiValue(string query, string propertyName)
    {
        try
        {
            using var searcher = new System.Management.ManagementObjectSearcher(query);
            foreach (var item in searcher.Get().Cast<System.Management.ManagementBaseObject>())
            {
                return item[propertyName]?.ToString();
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
