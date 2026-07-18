using AivoraPOS.Core.Models.Settings;

namespace AivoraPOS.Core.Interfaces.Services;

public interface ISettingsService
{
    T Get<T>(string key, T defaultValue);
    Task SetAsync(string key, object value, CancellationToken cancellationToken = default);
    Task<BusinessInfoDto> GetBusinessInfoAsync(CancellationToken cancellationToken = default);
    Task SaveBusinessInfoAsync(BusinessInfoDto dto, CancellationToken cancellationToken = default);
    Task<ReceiptSettingsDto> GetReceiptSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveReceiptSettingsAsync(ReceiptSettingsDto dto, CancellationToken cancellationToken = default);
    Task<PosBehaviorDto> GetPosBehaviorAsync(CancellationToken cancellationToken = default);
    Task SavePosBehaviorAsync(PosBehaviorDto dto, CancellationToken cancellationToken = default);
    Task InvalidateCacheAsync(CancellationToken cancellationToken = default);
}
