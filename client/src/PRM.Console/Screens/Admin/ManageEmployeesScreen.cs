using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ManageEmployeesScreen
{
    private readonly ApiClient _apiClient;

    public ManageEmployeesScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("MANAGE EMPLOYEES");

            System.Console.WriteLine(" 1. View All Employees");
            System.Console.WriteLine(" 2. Update Employee");
            System.Console.WriteLine(" 3. Deactivate Employee");
            System.Console.WriteLine(" 4. Manage Employee Skills");
            System.Console.WriteLine(" 5. Assign Manager");
            System.Console.WriteLine(" 6. Back");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option");

            switch (option)
            {
                case "1":
                    var viewAllEmployeesScreen = new ViewAllEmployeesScreen(_apiClient);
                    await viewAllEmployeesScreen.ShowAsync();
                    break;
                case "2":
                    var updateEmployeeScreen = new UpdateEmployeeScreen(_apiClient);
                    await updateEmployeeScreen.ShowAsync();
                    break;
                case "3":
                    var deactivateEmployeeScreen = new DeactivateEmployeeScreen(_apiClient);
                    await deactivateEmployeeScreen.ShowAsync();
                    break;
                case "4":
                    var manageSkillsScreen = new ManageSkillsScreen(_apiClient);
                    await manageSkillsScreen.ShowAsync();
                    break;
                case "5":
                    var assignManagerScreen = new AssignManagerScreen(_apiClient);
                    await assignManagerScreen.ShowAsync();
                    break;
                case "6":
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option.");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }
}
