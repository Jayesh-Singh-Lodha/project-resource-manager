using PRM.Console.Helpers;
using PRM.Console.Models.Projects;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class CreateProjectScreen
{
    private readonly ApiClient _apiClient;

    public CreateProjectScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("CREATE PROJECT");

        var name = ConsoleHelper.Prompt("Project Name");
        var desc = ConsoleHelper.Prompt("Description");
        
        var sdStr = ConsoleHelper.Prompt("Start Date (yyyy-mm-dd)");
        DateTime sd = DateTime.TryParse(sdStr, out var parsedSd) ? parsedSd : DateTime.Now;

        var edStr = ConsoleHelper.Prompt("End Date (yyyy-mm-dd)");
        DateTime ed = DateTime.TryParse(edStr, out var parsedEd) ? parsedEd : DateTime.Now.AddMonths(1);

        var mgrIdStr = ConsoleHelper.Prompt("Project Manager ID");
        int? mgrId = int.TryParse(mgrIdStr, out int mId) ? mId : null;

        var request = new CreateProjectRequest(name, desc, sd, ed, "Planned", mgrId, 0);

        try
        {
            var response = await _apiClient.PostAsync<CreateProjectRequest, ProjectResponse>("api/projects", request);
            ConsoleHelper.WriteSuccess($"Project '{response.Name}' created successfully with ID {response.Id}.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to create project: {ex.Message}");
        }
        ConsoleHelper.WaitForKey();
    }
}
