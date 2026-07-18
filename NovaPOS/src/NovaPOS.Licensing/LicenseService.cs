using Microsoft.Extensions.Logging;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Extensions;
using NovaPOS.Core.Interfaces.Licensing;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Models;
using NovaPOS.Core.Models.Settings;
using NovaPOS.Licensing.Constants;

namespace NovaPOS.Licensing;

public sealed class LicenseService : ILicenseService
{
    private readonly ILicenseInfoRepository _licenseInfoRepository;
    private readonly LicenseCache _licenseCache;
    private readonly ILogger<LicenseService> _logger;
    private readonly byte[] _verificationKey;
    private readonly HttpClient _httpClient;

    private LicenseCheckResult? _lastCheckResult;

    public LicenseService(
        ILicenseInfoRepository licenseInfoRepository,
        LicenseCache licenseCache,
        ILogger<LicenseService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _licenseInfoRepository = licenseInfoRepository;
        _licenseCache = licenseCache;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient(nameof(LicenseService));
        _verificationKey = LicenseConstants.DefaultVerificationKey;
    }

    public LicenseStatus CurrentStatus => _lastCheckResult?.Status ?? LicenseStatus.Invalid;
    public LicensePlan EffectivePlan => _lastCheckResult?.EffectivePlan ?? LicensePlan.Starter;
    public bool IsTrial => _lastCheckResult?.IsTrial ?? false;
    public int? MaxProducts => IsTrial ? LicenseConstants.TrialMaxProducts : EffectivePlan.GetMaxProducts();
    public int? MaxCashiers => EffectivePlan.GetMaxCashiers();
    public bool ShowReceiptWatermark => IsTrial;
    public int? TrialDaysRemaining => _lastCheckResult?.TrialDaysRemaining;
    public DateTime? ExpiresAt => _lastCheckResult?.ExpiresAt;
    public string? MachineId { get; private set; }

    public async Task<LicenseCheckResult> ValidateOnLaunchAsync(CancellationToken cancellationToken = default)
    {
        MachineId = HardwareFingerprint.Generate().Hash;
        var license = await _licenseInfoRepository.GetAsync(cancellationToken);
        var fingerprint = HardwareFingerprint.Generate();
        var cache = _licenseCache.Read();
        var isOnline = await IsOnlineAsync(cancellationToken);

        if (license is null)
        {
            _lastCheckResult = EvaluateTrial(cache, fingerprint);
            _licenseCache.Write(CreateCacheEntry(_lastCheckResult, fingerprint, cache?.TrialStartedUtc ?? DateTime.UtcNow));
            return _lastCheckResult;
        }

        if (!LicenseKeyCodec.TryParse(license.LicenseKey, _verificationKey, out var payload))
        {
            return await FinalizeAsync(
                CreateResult(LicenseStatus.Invalid, license.Plan, license.ExpiresAt, false, null,
                    "The license key is invalid or has been tampered with.", false, false),
                fingerprint, cache, isOnline, null);
        }

        if (!IsHardwareAuthorized(license.HardwareFingerprint, fingerprint))
        {
            return await FinalizeAsync(
                CreateResult(LicenseStatus.Invalid, payload.Plan, payload.ExpiresAtUtc, false, null,
                    "This license is locked to a different machine.", false, false),
                fingerprint, cache, isOnline, null);
        }

        if (LicenseValidator.IsExpired(payload.ExpiresAtUtc))
        {
            return await FinalizeAsync(
                CreateResult(LicenseStatus.Expired, payload.Plan, payload.ExpiresAtUtc, false, null,
                    "Your license has expired. Renew your subscription to continue using all features.",
                    true, false, readOnly: true),
                fingerprint, cache, isOnline, null);
        }

        var validResult = CreateResult(
            LicenseStatus.Valid,
            payload.Plan,
            payload.ExpiresAtUtc,
            false,
            null,
            "License is valid.",
            true,
            true);

        return await FinalizeAsync(validResult, fingerprint, cache, isOnline, null);
    }

    public async Task<LicenseActivationResult> ActivateAsync(
        string licenseKey,
        string businessName,
        CancellationToken cancellationToken = default)
    {
        if (!LicenseKeyCodec.TryParse(licenseKey, _verificationKey, out var payload))
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = "The license key is invalid or has been tampered with."
            };
        }

        if (LicenseValidator.IsExpired(payload.ExpiresAtUtc))
        {
            return new LicenseActivationResult
            {
                Success = false,
                Message = "This license key has already expired."
            };
        }

        var fingerprint = HardwareFingerprint.Generate();
        var formattedKey = FormatLicenseKey(licenseKey);

        var licenseInfo = new LicenseInfo
        {
            LicenseKey = formattedKey,
            BusinessName = businessName.Trim(),
            ActivatedAt = DateTime.UtcNow,
            ExpiresAt = payload.ExpiresAtUtc,
            HardwareFingerprint = HardwareFingerprint.Serialize(fingerprint),
            Plan = payload.Plan,
            IsValid = true
        };

        await _licenseInfoRepository.SaveAsync(licenseInfo, cancellationToken);

        _lastCheckResult = CreateResult(
            LicenseStatus.Valid,
            payload.Plan,
            payload.ExpiresAtUtc,
            false,
            null,
            "License activated successfully.",
            true,
            true);

        _licenseCache.Write(CreateCacheEntry(_lastCheckResult, fingerprint, null));

        _logger.LogInformation("License activated for {BusinessName} on plan {Plan}.", businessName, payload.Plan);

        return new LicenseActivationResult
        {
            Success = true,
            Message = "License activated successfully.",
            Plan = payload.Plan,
            ExpiresAt = payload.ExpiresAtUtc
        };
    }

    public bool CanUse(LicenseFeature feature)
    {
        if (_lastCheckResult is null)
        {
            return false;
        }

        return _lastCheckResult.Status switch
        {
            LicenseStatus.Invalid => false,
            LicenseStatus.Expired => false,
            LicenseStatus.Trial or LicenseStatus.Valid or LicenseStatus.GracePeriod => EffectivePlan.Supports(feature),
            _ => false
        };
    }

    public async Task<LicenseDetailsDto> GetLicenseDetailsAsync(CancellationToken cancellationToken = default)
    {
        MachineId ??= HardwareFingerprint.Generate().Hash;
        var license = await _licenseInfoRepository.GetAsync(cancellationToken);

        return new LicenseDetailsDto
        {
            PlanName = IsTrial ? "Trial" : EffectivePlan.ToString(),
            ExpiresAt = ExpiresAt,
            MachineId = MachineId,
            LicensedBusinessName = license?.BusinessName,
            IsTrial = IsTrial,
            TrialDaysRemaining = TrialDaysRemaining
        };
    }

    public async Task<bool> TransferLicenseAsync(CancellationToken cancellationToken = default)
    {
        var license = await _licenseInfoRepository.GetAsync(cancellationToken);
        if (license is null)
        {
            return false;
        }

        _licenseCache.Clear();
        await _licenseInfoRepository.DeleteAsync(cancellationToken);

        _lastCheckResult = null;
        await ValidateOnLaunchAsync(cancellationToken);
        _logger.LogInformation("License transferred off this machine.");
        return true;
    }

    private Task<LicenseCheckResult> FinalizeAsync(
        LicenseCheckResult result,
        HardwareFingerprint.FingerprintData fingerprint,
        LicenseCache.CacheEntry? cache,
        bool isOnline,
        DateTime? trialStartedUtc)
    {
        if (result.Status is LicenseStatus.Valid)
        {
            _lastCheckResult = result;
            _licenseCache.Write(CreateCacheEntry(result, fingerprint, trialStartedUtc));
            return Task.FromResult(result);
        }

        if (!isOnline && cache is not null && _licenseCache.IsWithinGracePeriod(cache))
        {
            _lastCheckResult = CreateResult(
                LicenseStatus.GracePeriod,
                cache.Plan,
                cache.ExpiresAtUtc,
                cache.IsTrial,
                cache.IsTrial ? CalculateTrialDaysRemaining(cache.TrialStartedUtc) : null,
                "Running in offline grace period.",
                true,
                true);

            return Task.FromResult(_lastCheckResult);
        }

        _lastCheckResult = result;
        return Task.FromResult(result);
    }

    private LicenseCheckResult EvaluateTrial(LicenseCache.CacheEntry? cache, HardwareFingerprint.FingerprintData fingerprint)
    {
        var trialStarted = cache?.TrialStartedUtc ?? DateTime.UtcNow;
        var trialEnds = trialStarted.AddDays(LicenseConstants.TrialDays);
        var daysRemaining = CalculateTrialDaysRemaining(trialStarted);

        if (DateTime.UtcNow > trialEnds)
        {
            return CreateResult(
                LicenseStatus.Expired,
                LicensePlan.Starter,
                trialEnds,
                true,
                0,
                "Your 30-day trial has ended. Activate a license to continue.",
                true,
                false,
                readOnly: true);
        }

        return CreateResult(
            LicenseStatus.Trial,
            LicensePlan.Starter,
            trialEnds,
            true,
            daysRemaining,
            $"Trial mode: {daysRemaining} day(s) remaining.",
            true,
            true);
    }

    private static int? CalculateTrialDaysRemaining(DateTime? trialStartedUtc)
    {
        if (trialStartedUtc is null)
        {
            return LicenseConstants.TrialDays;
        }

        var remaining = (int)Math.Ceiling(trialStartedUtc.Value.AddDays(LicenseConstants.TrialDays).Subtract(DateTime.UtcNow).TotalDays);
        return Math.Max(remaining, 0);
    }

    private static bool IsHardwareAuthorized(string storedFingerprint, HardwareFingerprint.FingerprintData currentFingerprint)
    {
        try
        {
            var stored = HardwareFingerprint.Deserialize(storedFingerprint);
            return HardwareFingerprint.MatchesWithTolerance(
                stored,
                currentFingerprint,
                LicenseConstants.MaxComponentMismatches);
        }
        catch
        {
            return string.Equals(storedFingerprint, currentFingerprint.Hash, StringComparison.Ordinal);
        }
    }

    private async Task<bool> IsOnlineAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2));
            using var request = new HttpRequestMessage(HttpMethod.Head, "https://www.microsoft.com");
            using var response = await _httpClient.SendAsync(request, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private LicenseCache.CacheEntry CreateCacheEntry(
        LicenseCheckResult result,
        HardwareFingerprint.FingerprintData fingerprint,
        DateTime? trialStartedUtc) =>
        new(
            DateTime.UtcNow,
            result.Status,
            result.EffectivePlan,
            result.ExpiresAt,
            result.IsTrial,
            result.IsTrial ? trialStartedUtc ?? DateTime.UtcNow : null,
            HardwareFingerprint.Serialize(fingerprint));

    private static LicenseCheckResult CreateResult(
        LicenseStatus status,
        LicensePlan plan,
        DateTime? expiresAt,
        bool isTrial,
        int? trialDaysRemaining,
        string message,
        bool canRun,
        bool canUse,
        bool readOnly = false) =>
        new()
        {
            Status = status,
            EffectivePlan = plan,
            ExpiresAt = expiresAt,
            IsTrial = isTrial,
            TrialDaysRemaining = trialDaysRemaining,
            Message = message,
            CanRunApplication = canRun,
            CanUseFeatures = canUse,
            IsReadOnlyMode = readOnly
        };

    private static string FormatLicenseKey(string licenseKey)
    {
        var normalized = licenseKey.Trim().ToUpperInvariant().Replace(" ", string.Empty);
        var body = normalized.Replace($"{LicenseConstants.KeyPrefix}-", string.Empty).Replace("-", string.Empty);
        return LicenseKeyCodec.FormatKey(body);
    }
}
