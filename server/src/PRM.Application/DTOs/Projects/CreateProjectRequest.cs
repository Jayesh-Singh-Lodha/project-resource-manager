namespace PRM.Application.DTOs.Projects;

public record CreateProjectRequest(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int? ManagerId,
    int TotalStoryPoints
);
