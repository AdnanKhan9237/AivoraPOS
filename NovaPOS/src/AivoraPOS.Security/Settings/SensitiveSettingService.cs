using AivoraPOS.Core.Entities;
using AivoraPOS.Core.Interfaces.Repositories;
using AivoraPOS.Core.Interfaces.Security;

namespace AivoraPOS.Security.Settings;

public sealed class SensitiveSettingService
{
    private readonly IAppSettingRepository _appSettingRepository;
    private readonly IEncryptionService _encryptionService;

    public SensitiveSettingService(IAppSettingRepository appSettingRepository, IEncryptionService encryptionService)
    {
        _appSettingRepository = appSettingRepository;
        _encryptionService = encryptionService;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _appSettingRepository.GetByKeyAsync(key, cancellationToken);
        if (setting is null)
        {
            return null;
        }

        return setting.IsSensitive ? _encryptionService.Decrypt(setting.Value) : setting.Value;
    }

    public async Task SaveAsync(string key, string value, bool isSensitive, CancellationToken cancellationToken = default)
    {
        var storedValue = isSensitive ? _encryptionService.Encrypt(value) : value;
        var existing = await _appSettingRepository.GetByKeyAsync(key, cancellationToken);

        if (existing is null)
        {
            await _appSettingRepository.AddAsync(new AppSetting
            {
                Key = key,
                Value = storedValue,
                IsSensitive = isSensitive
            }, cancellationToken);
        }
        else
        {
            existing.Value = storedValue;
            existing.IsSensitive = isSensitive;
            await _appSettingRepository.UpdateAsync(existing, cancellationToken);
        }

        await _appSettingRepository.SaveChangesAsync(cancellationToken);
    }
}
