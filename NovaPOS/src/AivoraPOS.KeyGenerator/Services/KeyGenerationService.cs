using System.IO;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using AivoraPOS.Core.Enums;
using AivoraPOS.Core.Models;
using AivoraPOS.KeyGenerator.Data;
using AivoraPOS.KeyGenerator.Entities;
using AivoraPOS.Licensing;
using AivoraPOS.Licensing.Constants;

namespace AivoraPOS.KeyGenerator.Services;

public sealed class KeyGenerationService
{
    private readonly IDbContextFactory<KeyGeneratorDbContext> _dbContextFactory;
    private readonly byte[] _signingKey;

    public KeyGenerationService(IDbContextFactory<KeyGeneratorDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
        _signingKey = LoadSigningKey();
    }

    public async Task<GeneratedLicenseKey> GenerateAsync(
        string customerName,
        LicensePlan plan,
        DateTime expiryDateUtc,
        CancellationToken cancellationToken = default)
    {
        var payload = new LicenseKeyPayload
        {
            Plan = plan,
            ExpiresAtUtc = expiryDateUtc.ToUniversalTime(),
            Salt = (uint)RandomNumberGenerator.GetInt32(int.MaxValue)
        };

        var licenseKey = LicenseKeyCodec.Generate(payload, _signingKey);

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var record = new GeneratedLicenseKey
        {
            CustomerName = customerName.Trim(),
            LicenseKey = licenseKey,
            Plan = plan,
            ExpiresAtUtc = payload.ExpiresAtUtc,
            GeneratedAtUtc = DateTime.UtcNow
        };

        db.GeneratedLicenseKeys.Add(record);
        await db.SaveChangesAsync(cancellationToken);
        return record;
    }

    public async Task<List<GeneratedLicenseKey>> GetHistoryAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await db.GeneratedLicenseKeys
            .AsNoTracking()
            .OrderByDescending(x => x.GeneratedAtUtc)
            .ToListAsync(cancellationToken);
    }

    private static byte[] LoadSigningKey()
    {
        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AivoraPOS",
            "KeyGenerator",
            "signing.key");

        if (File.Exists(configPath))
        {
            return File.ReadAllBytes(configPath);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
        var key = LicenseConstants.DefaultVerificationKey;
        File.WriteAllBytes(configPath, key);
        return key;
    }
}
