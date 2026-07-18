using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace AivoraPOS.Security.Infrastructure;

internal static class MachineIdentity
{
    internal static string GetFingerprintHash()
    {
        var cpuId = ReadWmiValue("SELECT ProcessorId FROM Win32_Processor", "ProcessorId") ?? Environment.MachineName;
        var boardSerial = ReadWmiValue("SELECT SerialNumber FROM Win32_BaseBoard", "SerialNumber") ?? "UNKNOWN-BOARD";
        var windowsId = ReadWindowsInstallationId();
        var combined = $"{cpuId}|{boardSerial}|{windowsId}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(combined)))[..32];
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
            return key?.GetValue("MachineGuid")?.ToString() ?? Environment.MachineName;
        }
        catch
        {
            return Environment.MachineName;
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadWmiValue(string query, string propertyName)
    {
        if (!OperatingSystem.IsWindows())
        {
            return null;
        }

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
