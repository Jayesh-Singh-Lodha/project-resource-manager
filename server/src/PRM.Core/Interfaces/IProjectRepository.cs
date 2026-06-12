using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Repository contract for Project and Milestone data access.
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<IReadOnlyList<Project>> GetAllAsync();
    Task<IReadOnlyList<Project>> GetByManagerIdAsync(int managerId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task<Milestone?> GetMilestoneByIdAsync(int id);
    Task AddMilestoneAsync(Milestone milestone);
    Task UpdateMilestoneAsync(Milestone milestone);
}
