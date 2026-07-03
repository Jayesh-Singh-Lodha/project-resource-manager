using PRM.Application.DTOs.Projects;
using PRM.Application.Interfaces;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
    {
        if (request.StartDate > request.EndDate)
        {
            throw new DomainException("Start date must be before end date.", "INVALID_DATE_RANGE");
        }

        if (!Enum.TryParse<ProjectStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new DomainException($"Invalid status '{request.Status}'.", "INVALID_STATUS");
        }

        if (request.ManagerId.HasValue)
        {
            var manager = await _userRepository.GetByIdAsync(request.ManagerId.Value);
            if (manager is null || manager.Role.Name != "Manager")
            {
                throw new DomainException("Assigned manager must be an existing user with the Manager role.", "INVALID_MANAGER");
            }
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = status,
            ManagerId = request.ManagerId,
            TotalStoryPoints = request.TotalStoryPoints,
            HealthStatus = HealthStatus.OnTrack,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        return MapToResponse(project);
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllAsync();
        return projects.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetProjectsByManagerIdAsync(int managerId)
    {
        var projects = await _projectRepository.GetByManagerIdAsync(managerId);
        return projects.Select(MapToResponse).ToList().AsReadOnly();
    }

    public async Task UpdateProjectAsync(int id, UpdateProjectRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project is null)
        {
            throw new DomainException("Project not found.", "PROJECT_NOT_FOUND");
        }

        if (request.StartDate > request.EndDate)
        {
            throw new DomainException("Start date must be before end date.", "INVALID_DATE_RANGE");
        }

        if (!Enum.TryParse<ProjectStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new DomainException($"Invalid status '{request.Status}'.", "INVALID_STATUS");
        }

        if (request.ManagerId.HasValue)
        {
            var manager = await _userRepository.GetByIdAsync(request.ManagerId.Value);
            if (manager is null || manager.Role.Name != "Manager")
            {
                throw new DomainException("Assigned manager must be an existing user with the Manager role.", "INVALID_MANAGER");
            }
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.Status = status;
        project.ManagerId = request.ManagerId;
        project.TotalStoryPoints = request.TotalStoryPoints;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project);
    }

    public async Task<ProjectResponse> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project is null)
        {
            throw new DomainException("Project not found.", "PROJECT_NOT_FOUND");
        }

        return MapToResponse(project);
    }

    public async Task AddMilestoneAsync(int projectId, AddMilestoneRequest request)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
        {
            throw new DomainException("Project not found.", "PROJECT_NOT_FOUND");
        }

        var milestone = new Milestone
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            DueDate = request.DueDate,
            StoryPoints = request.StoryPoints,
            Status = MilestoneStatus.NotStarted
        };

        await _projectRepository.AddMilestoneAsync(milestone);
    }

    public async Task UpdateMilestoneStatusAsync(int milestoneId, UpdateMilestoneStatusRequest request)
    {
        var milestone = await _projectRepository.GetMilestoneByIdAsync(milestoneId);
        if (milestone is null)
        {
            throw new DomainException("Milestone not found.", "MILESTONE_NOT_FOUND");
        }

        if (!Enum.TryParse<MilestoneStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new DomainException($"Invalid milestone status '{request.Status}'.", "INVALID_STATUS");
        }

        milestone.Status = status;
        await _projectRepository.UpdateMilestoneAsync(milestone);
    }

    public async Task<IReadOnlyList<MilestoneResponse>> GetMilestonesByProjectIdAsync(int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project is null)
        {
            throw new DomainException("Project not found.", "PROJECT_NOT_FOUND");
        }

        return project.Milestones.Select(m => new MilestoneResponse(
            Id: m.Id,
            ProjectId: m.ProjectId,
            Title: m.Title,
            DueDate: m.DueDate,
            StoryPoints: m.StoryPoints,
            Status: m.Status.ToString()
        )).ToList().AsReadOnly();
    }

    private static ProjectResponse MapToResponse(Project p)
    {
        var completedStoryPoints = p.Milestones
            .Where(m => m.Status == MilestoneStatus.Done)
            .Sum(m => m.StoryPoints);

        return new ProjectResponse(
            Id: p.Id,
            Name: p.Name,
            Description: p.Description,
            StartDate: p.StartDate,
            EndDate: p.EndDate,
            Status: p.Status.ToString(),
            ManagerId: p.ManagerId,
            ManagerName: p.Manager?.FullName,
            TotalStoryPoints: p.TotalStoryPoints,
            StoryPointsCompleted: completedStoryPoints,
            HealthStatus: p.HealthStatus.ToString(),
            CreatedAt: p.CreatedAt
        );
    }
}
