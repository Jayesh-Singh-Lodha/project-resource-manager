namespace PRM.Application.DTOs.Projects;

public record UpdateProjectRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? ManagerId,
    int TotalStoryPoints
);
