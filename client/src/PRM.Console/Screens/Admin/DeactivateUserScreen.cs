using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class DeactivateUserScreen
{
    private readonly ApiClient _apiClient;

    public DeactivateUserScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("DEACTIVATE USER");

        var idStr = ConsoleHelper.Prompt("Enter User ID to deactivate");
        if (!int.TryParse(idStr, out int userId))
        {
            ConsoleHelper.WriteError("Invalid User ID.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var confirm = ConsoleHelper.Prompt("Are you sure? This will end all active allocations. [Y/N]").ToUpper();
        if (confirm != "Y")
        {
            return;
        }

        try
        {
            await _apiClient.PostAsync($"api/admin/users/{userId}/deactivate", new { });
            ConsoleHelper.WriteSuccess("User deactivated successfully.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to deactivate user: {ex.Message}");
        }
        ConsoleHelper.WaitForKey();
    }
}
