using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class SystemConfigRepository : ISystemConfigRepository
{
    private readonly PrmDbContext _context;

    public SystemConfigRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<SystemConfig?> GetByKeyAsync(string key)
    {
        return await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key.ToLower() == key.ToLower());
    }

    public async Task<IReadOnlyList<SystemConfig>> GetAllAsync()
    {
        return await _context.SystemConfigs
            .OrderBy(c => c.Key)
            .ToListAsync();
    }

    public async Task AddOrUpdateAsync(SystemConfig config)
    {
        var existing = await _context.SystemConfigs
            .FirstOrDefaultAsync(c => c.Key.ToLower() == config.Key.ToLower());

        if (existing is null)
        {
            await _context.SystemConfigs.AddAsync(config);
        }
        else
        {
            existing.Value = config.Value;
            _context.SystemConfigs.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}
