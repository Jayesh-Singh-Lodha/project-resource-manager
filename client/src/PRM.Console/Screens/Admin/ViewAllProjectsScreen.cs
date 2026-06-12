using PRM.Console.Helpers;
using PRM.Console.Models.Projects;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ViewAllProjectsScreen
{
    private readonly ApiClient _apiClient;

    public ViewAllProjectsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ALL PROJECTS");

        try
        {
            var projects = await _apiClient.GetAsync<List<ProjectResponse>>("api/projects");

            System.Console.WriteLine("ID    Name                 Manager ID    Status");
            System.Console.WriteLine(new string('─', 50));

            foreach (var p in projects)
            {
                var mgrStr = p.ManagerId?.ToString() ?? "None";
                System.Console.WriteLine($"{p.Id,-5} {p.Name,-20} {mgrStr,-13} {p.Status}");
            }

            System.Console.WriteLine(new string('─', 50));
            System.Console.WriteLine($"Total Projects: {projects.Count}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load projects: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
