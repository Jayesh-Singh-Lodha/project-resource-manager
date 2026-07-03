namespace PRM.Application.DTOs.Projects;

public record ProjectResponse(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? ManagerId,
    string? ManagerName,
    int TotalStoryPoints,
    int StoryPointsCompleted,
    string HealthStatus,
    DateTime CreatedAt
);
