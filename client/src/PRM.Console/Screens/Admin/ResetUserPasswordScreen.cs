using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ResetUserPasswordScreen
{
    private readonly ApiClient _apiClient;

    public ResetUserPasswordScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("RESET USER PASSWORD");

        var idStr = ConsoleHelper.Prompt("Enter User ID");
        if (!int.TryParse(idStr, out int userId))
        {
            ConsoleHelper.WriteError("Invalid User ID.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var newPassword = ConsoleHelper.ReadPassword("Enter New Password");
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ConsoleHelper.WriteError("Password cannot be empty.");
            ConsoleHelper.WaitForKey();
            return;
        }

        try
        {
            var request = new ResetPasswordRequest(newPassword);
            await _apiClient.PostAsync($"api/admin/users/{userId}/reset-password", request);
            
            ConsoleHelper.WriteSuccess("Password reset successfully. User will be forced to change it on next login.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to reset password: {ex.Message}");
        }
        ConsoleHelper.WaitForKey();
    }
}
