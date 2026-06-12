namespace PRM.Console.Models.Projects;

public record AddMilestoneRequest(
    string Title,
    DateTime DueDate,
    int StoryPoints
);
