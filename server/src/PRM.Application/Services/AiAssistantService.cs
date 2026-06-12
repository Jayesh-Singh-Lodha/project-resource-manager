using PRM.Application.Interfaces;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

public class AiAssistantService : IAiAssistantService
{
    private readonly IEnumerable<ILlmProvider> _providers;
    private readonly ISystemConfigService _configService;
    private readonly IProjectService _projectService;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;

    public AiAssistantService(
        IEnumerable<ILlmProvider> providers,
        ISystemConfigService configService,
        IProjectService projectService,
        IUserService userService,
        IUserRepository userRepository)
    {
        _providers = providers;
        _configService = configService;
        _projectService = projectService;
        _userService = userService;
        _userRepository = userRepository;
    }

    private async Task<ILlmProvider> GetActiveProviderAsync()
    {
        var config = await _configService.GetConfigByKeyAsync("LlmProvider");
        var activeProviderName = config?.Value ?? "Gemini";
        var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(activeProviderName, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            throw new DomainException($"LLM provider '{activeProviderName}' is not configured or not supported.", "LLM_PROVIDER_NOT_FOUND");
        }
        return provider;
    }

    public async Task<string> GetProjectRiskSummaryAsync(int projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        
        var systemPrompt = "You are an AI project manager assistant. Analyze the given project details and provide a brief risk summary.";
        var userPrompt = $"Project Name: {project.Name}\nStatus: {project.Status}\nHealth: {project.HealthStatus}\nTotal Story Points: {project.TotalStoryPoints}\nCompleted Story Points: {project.StoryPointsCompleted}\nStart Date: {project.StartDate}\nEnd Date: {project.EndDate}\n\nPlease analyze the risk of this project not finishing on time.";
        
        var provider = await GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(systemPrompt, userPrompt);
    }

    public async Task<string> AskGeneralQuestionAsync(string question)
    {
        var systemPrompt = "You are a helpful AI assistant for the Project and Resource Management tool.";
        var provider = await GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(systemPrompt, question);
    }

    public async Task<string> SearchResourcesAsync(string criteria, int managerId)
    {
        var allCandidates = await _userRepository.GetEmployeesWithDetailsAsync();
        var candidates = allCandidates.ToList();
        
        var userData = string.Join("\n\n", candidates.Select(u => 
        {
            var skills = u.UserSkills != null && u.UserSkills.Any() 
                ? string.Join(", ", u.UserSkills.Select(us => $"{us.Skill.Name} ({us.ProficiencyLevel})")) 
                : "None";
            
            var allocations = u.Allocations != null && u.Allocations.Any(a => a.ToDate >= DateTime.UtcNow.Date)
                ? string.Join(", ", u.Allocations.Where(a => a.ToDate >= DateTime.UtcNow.Date).Select(a => $"Project: {a.Project?.Name ?? a.ProjectId.ToString()} ({a.UtilisationPercent}%) until {a.ToDate:yyyy-MM-dd}"))
                : "No active allocations (Available)";

            return $"ID: {u.Id}\nName: {u.FullName}\nDept: {u.Department}\nStatus: {u.Status}\nSkills: {skills}\nAllocations: {allocations}";
        }));

        var systemPrompt = "You are an AI resource allocation assistant. You have access to a list of employees with their specific skills, proficiencies, and current project allocations/availability. " +
                           "Based on the user's criteria, carefully evaluate each employee's skills and availability to recommend the best matches. " +
                           "Return ONLY a short summary of recommendations and the employee IDs.";
                           
        var userPrompt = $"Criteria: {criteria}\n\nEmployees:\n{userData}";
        
        var provider = await GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(systemPrompt, userPrompt);
    }

    public async Task<string> BuildTeamAsync(string teamRequirements, int managerId)
    {
        var allCandidates = await _userRepository.GetEmployeesWithDetailsAsync();
        var candidates = allCandidates.ToList();
        
        var userData = string.Join("\n\n", candidates.Select(u => 
        {
            var skills = u.UserSkills != null && u.UserSkills.Any() 
                ? string.Join(", ", u.UserSkills.Select(us => $"{us.Skill.Name} ({us.ProficiencyLevel})")) 
                : "None";
            
            var allocations = u.Allocations != null && u.Allocations.Any(a => a.ToDate >= DateTime.UtcNow.Date)
                ? string.Join(", ", u.Allocations.Where(a => a.ToDate >= DateTime.UtcNow.Date).Select(a => $"Project: {a.Project?.Name ?? a.ProjectId.ToString()} ({a.UtilisationPercent}%) until {a.ToDate:yyyy-MM-dd}"))
                : "No active allocations (Available)";

            return $"ID: {u.Id}\nName: {u.FullName}\nDept: {u.Department}\nStatus: {u.Status}\nSkills: {skills}\nAllocations: {allocations}";
        }));

        var systemPrompt = "You are an AI team builder for the Project and Resource Management tool. " +
                           "You have access to a list of employees with their specific skills, proficiencies, and current project allocations/availability.\n" +
                           "The user will describe a whole team they need, potentially with multiple roles and required skills.\n" +
                           "Your task is to:\n" +
                           "1. Perform a single-pass best match to fill every role requested with the best available bench employee in one go.\n" +
                           "2. NEVER put the same person in two different roles for this team.\n" +
                           "3. Be honest about gaps: when a role cannot be filled, tell the manager exactly which role and precisely why.\n" +
                           "4. There are two kinds of \"why\" for gaps: either nobody has the skill (so hire or train), or someone has it but is allocated elsewhere until a date (so plan around their availability).\n" +
                           "Return a clear, formatted summary of the recommended team members and any gaps.";
                           
        var userPrompt = $"Team Requirements: {teamRequirements}\n\nEmployees:\n{userData}";
        
        var provider = await GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(systemPrompt, userPrompt);
    }

    public async Task<string> GetSuggestedHelpForProjectAsync(int projectId)
    {
        var project = await _projectService.GetProjectByIdAsync(projectId);
        
        var allCandidates = await _userRepository.GetEmployeesWithDetailsAsync();
        var candidates = allCandidates.ToList();
        
        var userData = string.Join("\n\n", candidates.Select(u => 
        {
            var skills = u.UserSkills != null && u.UserSkills.Any() 
                ? string.Join(", ", u.UserSkills.Select(us => $"{us.Skill.Name} ({us.ProficiencyLevel})")) 
                : "None";
            
            var allocations = u.Allocations != null && u.Allocations.Any(a => a.ToDate >= DateTime.UtcNow.Date)
                ? string.Join(", ", u.Allocations.Where(a => a.ToDate >= DateTime.UtcNow.Date).Select(a => $"Project: {a.Project?.Name ?? a.ProjectId.ToString()} ({a.UtilisationPercent}%) until {a.ToDate:yyyy-MM-dd}"))
                : "No active allocations (Available)";

            return $"ID: {u.Id}\nName: {u.FullName}\nDept: {u.Department}\nStatus: {u.Status}\nSkills: {skills}\nAllocations: {allocations}";
        }));

        var systemPrompt = "You are an AI resource allocation assistant for a Project Management Tool. " +
                           "A project has just been flagged as 'At Risk'. Analyze the project details and the list of employees, " +
                           "and suggest 1 to 3 employees whose skills might help mitigate the risk. " +
                           "Focus on employees who have availability or are on the bench. " +
                           "Provide a brief, plain-English explanation for why each employee is recommended.";

        var userPrompt = $"Project Name: {project.Name}\nDescription: {project.Description}\nStatus: {project.Status}\n\nEmployees:\n{userData}";

        var provider = await GetActiveProviderAsync();
        return await provider.GenerateResponseAsync(systemPrompt, userPrompt);
    }
}
