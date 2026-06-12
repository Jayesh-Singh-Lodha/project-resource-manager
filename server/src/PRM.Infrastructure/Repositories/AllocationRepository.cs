using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

public class AllocationRepository : IAllocationRepository
{
    private readonly PrmDbContext _context;

    public AllocationRepository(PrmDbContext context)
    {
        _context = context;
    }

    public async Task<Allocation?> GetByIdAsync(int id)
    {
        return await _context.Allocations
            .Include(a => a.User)
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IReadOnlyList<Allocation>> GetAllAsync()
    {
        return await _context.Allocations
            .Include(a => a.User)
            .Include(a => a.Project)
            .OrderBy(a => a.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Allocation>> GetByEmployeeIdAsync(int employeeId)
    {
        return await _context.Allocations
            .Include(a => a.User)
            .Include(a => a.Project)
            .Where(a => a.UserId == employeeId)
            .OrderByDescending(a => a.FromDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Allocation>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Allocations
            .Include(a => a.User)
            .Include(a => a.Project)
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.FromDate)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Allocation>> GetOverlappingAllocationsAsync(int userId, DateTime fromDate, DateTime toDate)
    {
        return await _context.Allocations
            .Where(a => a.UserId == userId &&
                        a.FromDate <= toDate &&
                        a.ToDate >= fromDate)
            .ToListAsync();
    }

    public async Task AddAsync(Allocation allocation)
    {
        await _context.Allocations.AddAsync(allocation);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Allocation allocation)
    {
        _context.Allocations.Update(allocation);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Allocation allocation)
    {
        _context.Allocations.Remove(allocation);
        await _context.SaveChangesAsync();
    }
}
