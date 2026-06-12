namespace PRM.Application.DTOs.Projects;

public record AddMilestoneRequest(
    string Title,
    DateTime DueDate,
    int StoryPoints
);
