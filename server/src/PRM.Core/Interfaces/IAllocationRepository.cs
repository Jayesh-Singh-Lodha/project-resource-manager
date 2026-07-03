using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Repository contract for resource Allocation data access.
/// </summary>
public interface IAllocationRepository
{
    Task<Allocation?> GetByIdAsync(int id);
    Task<IReadOnlyList<Allocation>> GetAllAsync();
    Task<IReadOnlyList<Allocation>> GetByEmployeeIdAsync(int employeeId);
    Task<IReadOnlyList<Allocation>> GetByProjectIdAsync(int projectId);
    Task<IReadOnlyList<Allocation>> GetOverlappingAllocationsAsync(int userId, DateTime fromDate, DateTime toDate);
    Task AddAsync(Allocation allocation);
    Task UpdateAsync(Allocation allocation);
    Task DeleteAsync(Allocation allocation);
}
