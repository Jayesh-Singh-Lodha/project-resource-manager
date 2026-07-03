using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Manager;

public class ManagerMenuScreen
{
    private readonly ApiClient _apiClient;
    private readonly string _fullName;

    public ManagerMenuScreen(ApiClient apiClient, string fullName)
    {
        _apiClient = apiClient;
        _fullName = fullName;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox(
                $"MANAGER MENU — Welcome, {_fullName}",
                "Select an option below to proceed.");

            System.Console.WriteLine(" [1] Resource Dashboard");
            System.Console.WriteLine(" [2] Allocate Resources");
            System.Console.WriteLine(" [3] My Projects");
            System.Console.WriteLine(" [4] Team Timesheets");
            System.Console.WriteLine(" [5] AI Assistant");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine(" [L] Logout");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option").ToUpper();

            switch (option)
            {
                case "1":
                    await new ResourceDashboardScreen(_apiClient).ShowAsync();
                    break;
                case "2":
                    await new AllocateResourceScreen(_apiClient).ShowAsync();
                    break;
                case "3":
                    await new MyProjectsScreen(_apiClient).ShowAsync();
                    break;
                case "4":
                    await new TeamTimesheetsScreen(_apiClient).ShowAsync();
                    break;
                case "5":
                    await new AiAssistantScreen(_apiClient).ShowAsync();
                    break;
                case "L":
                    _apiClient.ClearToken();
                    ConsoleHelper.WriteSuccess("Logged out successfully.");
                    ConsoleHelper.WaitForKey();
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option. Please try again.");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }
}
