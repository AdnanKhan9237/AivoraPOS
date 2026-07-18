using System.Globalization;
using Microsoft.Extensions.Logging;
using NovaPOS.Core.Constants;
using NovaPOS.Core.Entities;
using NovaPOS.Core.Enums;
using NovaPOS.Core.Interfaces.Repositories;
using NovaPOS.Core.Interfaces.Security;
using NovaPOS.Core.Interfaces.Services;
using NovaPOS.Core.Models.Settings;

namespace NovaPOS.Data.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IAppSettingRepository _appSettingRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<SettingsService> _logger;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private Dictionary<string, string>? _cache;

    public SettingsService(
        IAppSettingRepository appSettingRepository,
        IAuditService auditService,
        ILogger<SettingsService> logger)
    {
        _appSettingRepository = appSettingRepository;
        _auditService = auditService;
        _logger = logger;
    }

    public T Get<T>(string key, T defaultValue)
    {
        EnsureLoadedSync();
        if (_cache is null || !_cache.TryGetValue(key, out var raw))
        {
            return defaultValue;
        }

        return ConvertValue(raw, defaultValue);
    }

    public async Task SetAsync(string key, object value, CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        var stringValue = ConvertToString(value);
        var oldValue = _cache!.TryGetValue(key, out var existing) ? existing : null;

        var setting = await _appSettingRepository.GetByKeyAsync(key, cancellationToken);
        if (setting is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = key,
                Value = stringValue
            }, cancellationToken);
        }
        else
        {
            setting.Value = stringValue;
            setting.UpdatedAt = DateTime.UtcNow;
            await _appSettingRepository.UpdateAsync(setting, cancellationToken);
        }

        await _appSettingRepository.SaveChangesAsync(cancellationToken);
        _cache[key] = stringValue;

        await _auditService.LogAsync(
            "SettingsChanged",
            entityType: "AppSetting",
            entityId: key,
            oldValues: new { Key = key, Value = oldValue },
            newValues: new { Key = key, Value = stringValue },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Setting {Key} updated.", key);
    }

    public async Task<BusinessInfoDto> GetBusinessInfoAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        return new BusinessInfoDto
        {
            Name = Get(SettingKeys.StoreName, "NovaPOS Store"),
            Address = Get(SettingKeys.StoreAddress, string.Empty),
            Phone = Get(SettingKeys.StorePhone, string.Empty),
            LogoPath = Get<string?>(SettingKeys.StoreLogoPath, null),
            CurrencySymbol = Get(SettingKeys.CurrencySymbol, "$"),
            CurrencyPosition = Get(SettingKeys.CurrencyPosition, CurrencyPosition.Before),
            DefaultTaxRate = Get(SettingKeys.DefaultTaxRate, 0.0825m)
        };
    }

    public async Task SaveBusinessInfoAsync(BusinessInfoDto dto, CancellationToken cancellationToken = default)
    {
        await SetAsync(SettingKeys.StoreName, dto.Name, cancellationToken);
        await SetAsync(SettingKeys.StoreAddress, dto.Address, cancellationToken);
        await SetAsync(SettingKeys.StorePhone, dto.Phone, cancellationToken);
        if (!string.IsNullOrWhiteSpace(dto.LogoPath))
        {
            await SetAsync(SettingKeys.StoreLogoPath, dto.LogoPath, cancellationToken);
        }

        await SetAsync(SettingKeys.CurrencySymbol, dto.CurrencySymbol, cancellationToken);
        await SetAsync(SettingKeys.CurrencyPosition, dto.CurrencyPosition.ToString(), cancellationToken);
        await SetAsync(SettingKeys.DefaultTaxRate, dto.DefaultTaxRate.ToString(CultureInfo.InvariantCulture), cancellationToken);
    }

    public async Task<ReceiptSettingsDto> GetReceiptSettingsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        return new ReceiptSettingsDto
        {
            HeaderText = Get(SettingKeys.ReceiptHeader, string.Empty),
            FooterText = Get(SettingKeys.ReceiptFooter, "Thank you for your business!"),
            ShowLogo = Get(SettingKeys.ReceiptShowLogo, false),
            AutoPrint = Get(SettingKeys.ReceiptAutoPrint, false),
            PrinterName = Get(SettingKeys.PrinterName, string.Empty),
            Width = Get(SettingKeys.ReceiptWidth, ReceiptWidth.Mm80)
        };
    }

    public async Task SaveReceiptSettingsAsync(ReceiptSettingsDto dto, CancellationToken cancellationToken = default)
    {
        await SetAsync(SettingKeys.ReceiptHeader, dto.HeaderText, cancellationToken);
        await SetAsync(SettingKeys.ReceiptFooter, dto.FooterText, cancellationToken);
        await SetAsync(SettingKeys.ReceiptShowLogo, dto.ShowLogo, cancellationToken);
        await SetAsync(SettingKeys.ReceiptAutoPrint, dto.AutoPrint, cancellationToken);
        await SetAsync(SettingKeys.PrinterName, dto.PrinterName, cancellationToken);
        await SetAsync(SettingKeys.ReceiptWidth, ((int)dto.Width).ToString(CultureInfo.InvariantCulture), cancellationToken);
    }

    public async Task<PosBehaviorDto> GetPosBehaviorAsync(CancellationToken cancellationToken = default)
    {
        await EnsureLoadedAsync(cancellationToken);
        return new PosBehaviorDto
        {
            IdleLockTimeoutMinutes = Get(SettingKeys.IdleTimeoutMinutes, 5),
            RequireManagerForDiscount = Get(SettingKeys.RequireManagerForDiscount, false),
            AllowNegativeStock = Get(SettingKeys.AllowNegativeStock, false),
            DefaultPaymentMethod = Get(SettingKeys.DefaultPaymentMethod, PaymentMethod.Cash),
            SoundOnSaleComplete = Get(SettingKeys.SoundOnSaleComplete, true)
        };
    }

    public async Task SavePosBehaviorAsync(PosBehaviorDto dto, CancellationToken cancellationToken = default)
    {
        await SetAsync(SettingKeys.IdleTimeoutMinutes, dto.IdleLockTimeoutMinutes, cancellationToken);
        await SetAsync(SettingKeys.RequireManagerForDiscount, dto.RequireManagerForDiscount, cancellationToken);
        await SetAsync(SettingKeys.AllowNegativeStock, dto.AllowNegativeStock, cancellationToken);
        await SetAsync(SettingKeys.DefaultPaymentMethod, dto.DefaultPaymentMethod.ToString(), cancellationToken);
        await SetAsync(SettingKeys.SoundOnSaleComplete, dto.SoundOnSaleComplete, cancellationToken);
    }

    public Task InvalidateCacheAsync(CancellationToken cancellationToken = default)
    {
        _cache = null;
        return Task.CompletedTask;
    }

    private void EnsureLoadedSync()
    {
        if (_cache is not null)
        {
            return;
        }

        _loadLock.Wait();
        try
        {
            _cache ??= LoadCacheAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private async Task EnsureLoadedAsync(CancellationToken cancellationToken)
    {
        if (_cache is not null)
        {
            return;
        }

        await _loadLock.WaitAsync(cancellationToken);
        try
        {
            if (_cache is null)
            {
                _cache = await LoadCacheAsync(cancellationToken);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private async Task<Dictionary<string, string>> LoadCacheAsync(CancellationToken cancellationToken)
    {
        var settings = await _appSettingRepository.GetAllAsync(cancellationToken);
        return settings.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static string ConvertToString(object value) => value switch
    {
        bool b => b ? "true" : "false",
        decimal d => d.ToString(CultureInfo.InvariantCulture),
        double d => d.ToString(CultureInfo.InvariantCulture),
        float f => f.ToString(CultureInfo.InvariantCulture),
        Enum e => e.ToString(),
        _ => value?.ToString() ?? string.Empty
    };

    private static T ConvertValue<T>(string raw, T defaultValue)
    {
        try
        {
            var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (targetType == typeof(string))
            {
                return (T)(object)raw;
            }

            if (targetType == typeof(bool))
            {
                return (T)(object)bool.Parse(raw);
            }

            if (targetType == typeof(int))
            {
                return (T)(object)int.Parse(raw, CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(decimal))
            {
                return (T)(object)decimal.Parse(raw, CultureInfo.InvariantCulture);
            }

            if (targetType.IsEnum)
            {
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
                {
                    return (T)Enum.ToObject(targetType, numeric);
                }

                return (T)Enum.Parse(targetType, raw, true);
            }

            return (T)Convert.ChangeType(raw, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return defaultValue;
        }
    }
}
