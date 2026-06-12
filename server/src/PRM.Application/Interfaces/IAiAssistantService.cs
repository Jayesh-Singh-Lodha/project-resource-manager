namespace PRM.Application.Interfaces;

public interface IAiAssistantService
{
    Task<string> GetProjectRiskSummaryAsync(int projectId);
    Task<string> AskGeneralQuestionAsync(string question);
    Task<string> SearchResourcesAsync(string criteria, int managerId);
    Task<string> BuildTeamAsync(string teamRequirements, int managerId);
    Task<string> GetSuggestedHelpForProjectAsync(int projectId);
}
