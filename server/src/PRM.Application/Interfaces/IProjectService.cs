using PRM.Application.DTOs.Projects;

namespace PRM.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
    Task<IReadOnlyList<ProjectResponse>> GetAllProjectsAsync();
    Task<IReadOnlyList<ProjectResponse>> GetProjectsByManagerIdAsync(int managerId);
    Task UpdateProjectAsync(int id, UpdateProjectRequest request);
    Task<ProjectResponse> GetProjectByIdAsync(int id);
    Task AddMilestoneAsync(int projectId, AddMilestoneRequest request);
    Task UpdateMilestoneStatusAsync(int milestoneId, UpdateMilestoneStatusRequest request);
    Task<IReadOnlyList<MilestoneResponse>> GetMilestonesByProjectIdAsync(int projectId);
}
