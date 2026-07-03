using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ManageProjectsScreen
{
    private readonly ApiClient _apiClient;

    public ManageProjectsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("MANAGE PROJECTS");

            System.Console.WriteLine(" 1. Create Project");
            System.Console.WriteLine(" 2. View All Projects");
            System.Console.WriteLine(" 3. Update Project");
            System.Console.WriteLine(" 4. Manage Milestones");
            System.Console.WriteLine(" 5. Back");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option");

            switch (option)
            {
                case "1":
                    var createScreen = new CreateProjectScreen(_apiClient);
                    await createScreen.ShowAsync();
                    break;
                case "2":
                    var viewAllScreen = new ViewAllProjectsScreen(_apiClient);
                    await viewAllScreen.ShowAsync();
                    break;
                case "3":
                    var updateScreen = new UpdateProjectScreen(_apiClient);
                    await updateScreen.ShowAsync();
                    break;
                case "4":
                    var milestoneScreen = new ManageMilestonesScreen(_apiClient);
                    await milestoneScreen.ShowAsync();
                    break;
                case "5":
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option.");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }
}
