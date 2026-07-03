using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly PrmDbContext _context;

    public ProjectRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .Include(p => p.Allocations)
                .ThenInclude(a => a.User)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IReadOnlyList<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Project>> GetByManagerIdAsync(int managerId)
    {
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.Milestones)
            .Where(p => p.ManagerId == managerId)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    public async Task AddAsync(Project project)
    {
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task<Milestone?> GetMilestoneByIdAsync(int id)
    {
        return await _context.Milestones
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddMilestoneAsync(Milestone milestone)
    {
        await _context.Milestones.AddAsync(milestone);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateMilestoneAsync(Milestone milestone)
    {
        _context.Milestones.Update(milestone);
        await _context.SaveChangesAsync();
    }
}
