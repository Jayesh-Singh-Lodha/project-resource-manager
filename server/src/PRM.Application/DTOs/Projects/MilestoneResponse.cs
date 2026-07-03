namespace PRM.Application.DTOs.Projects;

public record MilestoneResponse(
    int Id,
    int ProjectId,
    string Title,
    DateTime DueDate,
    int StoryPoints,
    string Status
);
