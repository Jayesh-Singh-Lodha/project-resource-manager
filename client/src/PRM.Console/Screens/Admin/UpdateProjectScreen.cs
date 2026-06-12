using PRM.Console.Helpers;
using PRM.Console.Models.Projects;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class UpdateProjectScreen
{
    private readonly ApiClient _apiClient;

    public UpdateProjectScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("UPDATE PROJECT");

        var idStr = ConsoleHelper.Prompt("Enter Project ID");
        if (!int.TryParse(idStr, out int projectId)) return;

        try
        {
            var project = await _apiClient.GetAsync<ProjectResponse>($"api/projects/{projectId}");

            var newName = ConsoleHelper.Prompt($"Project Name [{project.Name}] (leave blank to keep)");
            var newDesc = ConsoleHelper.Prompt($"Description [{project.Description}] (leave blank to keep)");
            var status = ConsoleHelper.Prompt($"Status [{project.Status}] (Active, Completed, OnHold, Cancelled)");

            var request = new UpdateProjectRequest(
                string.IsNullOrWhiteSpace(newName) ? project.Name : newName,
                string.IsNullOrWhiteSpace(newDesc) ? project.Description : newDesc,
                project.StartDate,
                project.EndDate,
                string.IsNullOrWhiteSpace(status) ? project.Status : status,
                project.ManagerId,
                project.TotalStoryPoints
            );

            await _apiClient.PutAsync($"api/projects/{projectId}", request);
            ConsoleHelper.WriteSuccess("Project updated successfully.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to update project: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
