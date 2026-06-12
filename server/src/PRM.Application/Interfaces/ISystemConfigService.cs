using PRM.Application.DTOs.SystemConfig;

namespace PRM.Application.Interfaces;

public interface ISystemConfigService
{
    Task<IReadOnlyList<SystemConfigResponse>> GetAllConfigsAsync();
    Task<SystemConfigResponse?> GetConfigByKeyAsync(string key);
    Task UpdateConfigAsync(string key, string value);
}
