using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Repository contract for SystemConfig data access.
/// </summary>
public interface ISystemConfigRepository
{
    Task<SystemConfig?> GetByKeyAsync(string key);
    Task<IReadOnlyList<SystemConfig>> GetAllAsync();
    Task AddOrUpdateAsync(SystemConfig config);
}
