using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ManageUsersScreen
{
    private readonly ApiClient _apiClient;

    public ManageUsersScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("MANAGE USERS");

            System.Console.WriteLine(" 1. Create User Account");
            System.Console.WriteLine(" 2. View All Users");
            System.Console.WriteLine(" 3. Reset User Password");
            System.Console.WriteLine(" 4. Deactivate User");
            System.Console.WriteLine(" 5. Back");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option");

            switch (option)
            {
                case "1":
                    var createUserScreen = new CreateUserScreen(_apiClient);
                    await createUserScreen.ShowAsync();
                    break;
                case "2":
                    var viewAllUsersScreen = new ViewAllUsersScreen(_apiClient);
                    await viewAllUsersScreen.ShowAsync();
                    break;
                case "3":
                    var resetPasswordScreen = new ResetUserPasswordScreen(_apiClient);
                    await resetPasswordScreen.ShowAsync();
                    break;
                case "4":
                    var deactivateUserScreen = new DeactivateUserScreen(_apiClient);
                    await deactivateUserScreen.ShowAsync();
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
