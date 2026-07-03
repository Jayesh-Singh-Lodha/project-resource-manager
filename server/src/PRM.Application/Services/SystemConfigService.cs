using PRM.Application.DTOs.SystemConfig;
using PRM.Application.Interfaces;
using PRM.Core.Entities;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class SystemConfigService : ISystemConfigService
{
    private readonly ISystemConfigRepository _configRepository;

    public SystemConfigService(ISystemConfigRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task<IReadOnlyList<SystemConfigResponse>> GetAllConfigsAsync()
    {
        var configs = await _configRepository.GetAllAsync();
        return configs.Select(c => new SystemConfigResponse(c.Key, c.Value)).ToList().AsReadOnly();
    }

    public async Task<SystemConfigResponse?> GetConfigByKeyAsync(string key)
    {
        var config = await _configRepository.GetByKeyAsync(key);
        if (config is null) return null;
        return new SystemConfigResponse(config.Key, config.Value);
    }

    public async Task UpdateConfigAsync(string key, string value)
    {
        var config = new SystemConfig
        {
            Key = key.Trim(),
            Value = value.Trim()
        };
        await _configRepository.AddOrUpdateAsync(config);
    }
}
