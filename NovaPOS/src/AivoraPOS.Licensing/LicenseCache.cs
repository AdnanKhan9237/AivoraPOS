using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AivoraPOS.Core.Constants;
using AivoraPOS.Core.Enums;
using AivoraPOS.Licensing.Constants;

namespace AivoraPOS.Licensing;

public sealed class LicenseCache
{
    private static readonly string CacheFilePath =
        Path.Combine(AppPaths.AppDataRoot, LicenseConstants.CacheFileName);

    public sealed record CacheEntry(
        DateTime LastValidatedUtc,
        LicenseStatus Status,
        LicensePlan Plan,
        DateTime? ExpiresAtUtc,
        bool IsTrial,
        DateTime? TrialStartedUtc,
        string HardwareFingerprint);

    public CacheEntry? Read()
    {
        if (!File.Exists(CacheFilePath))
        {
            return null;
        }

        try
        {
            var encrypted = File.ReadAllText(CacheFilePath);
            var json = Decrypt(encrypted);
            return JsonSerializer.Deserialize<CacheEntry>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Write(CacheEntry entry)
    {
        AppPaths.EnsureDirectoriesExist();
        var json = JsonSerializer.Serialize(entry);
        var encrypted = Encrypt(json);
        File.WriteAllText(CacheFilePath, encrypted);
    }

    public void Clear()
    {
        if (File.Exists(CacheFilePath))
        {
            File.Delete(CacheFilePath);
        }
    }

    public bool IsWithinGracePeriod(CacheEntry entry)
    {
        return entry.LastValidatedUtc.AddDays(LicenseConstants.GracePeriodDays) >= DateTime.UtcNow;
    }

    private static string Encrypt(string plainText)
    {
        var key = DeriveKey();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        var payload = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(payload);
    }

    private static string Decrypt(string cipherText)
    {
        var payload = Convert.FromBase64String(cipherText);
        var key = DeriveKey();

        using var aes = Aes.Create();
        aes.Key = key;

        var iv = new byte[16];
        var cipherBytes = new byte[payload.Length - 16];
        Buffer.BlockCopy(payload, 0, iv, 0, 16);
        Buffer.BlockCopy(payload, 16, cipherBytes, 0, cipherBytes.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private static byte[] DeriveKey()
    {
        var fingerprint = HardwareFingerprint.Generate().Hash;
        return SHA256.HashData(Encoding.UTF8.GetBytes($"AivoraPOS-LicenseCache|{fingerprint}"));
    }
}
