using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// Main menu screen for Admin users.
/// Provides access to user management and other admin features.
/// </summary>
public class AdminMenuScreen
{
    private readonly ApiClient _apiClient;
    private readonly string _fullName;

    public AdminMenuScreen(ApiClient apiClient, string fullName)
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
                $"ADMIN MENU — Welcome, {_fullName}",
                "Select an option below to proceed.");

            System.Console.WriteLine(" 1. Manage Employees");
            System.Console.WriteLine(" 2. Manage Projects");
            System.Console.WriteLine(" 3. View All Allocations");
            System.Console.WriteLine(" 4. Manage Users");
            System.Console.WriteLine(" 5. System Configuration");
            System.Console.WriteLine(" 6. Logout");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option");

            switch (option)
            {
                case "1":
                    var manageEmployeesScreen = new ManageEmployeesScreen(_apiClient);
                    await manageEmployeesScreen.ShowAsync();
                    break;

                case "2":
                    var manageProjectsScreen = new ManageProjectsScreen(_apiClient);
                    await manageProjectsScreen.ShowAsync();
                    break;

                case "3":
                    var viewAllocationsScreen = new ViewAllAllocationsScreen(_apiClient);
                    await viewAllocationsScreen.ShowAsync();
                    break;

                case "4":
                    var manageUsersScreen = new ManageUsersScreen(_apiClient);
                    await manageUsersScreen.ShowAsync();
                    break;
                    
                case "5":
                    var systemConfigScreen = new SystemConfigScreen(_apiClient);
                    await systemConfigScreen.ShowAsync();
                    break;

                case "6":
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
