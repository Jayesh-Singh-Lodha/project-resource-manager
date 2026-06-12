using PRM.Console.Helpers;
using PRM.Console.Models.Auth;
using PRM.Console.Services;

namespace PRM.Console.Screens;

/// <summary>
/// Change Password screen — shown when ForcePasswordChange is true.
/// BRD: "This screen cannot be skipped. The application blocks access
/// to all menus until the password is changed."
/// </summary>
public class ChangePasswordScreen
{
    private readonly ApiClient _apiClient;

    public ChangePasswordScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Shows the change password prompt.
    /// Returns true if the password was changed successfully.
    /// </summary>
    public async Task<bool> ShowAsync()
    {
        ConsoleHelper.DrawBox(
            "CHANGE PASSWORD",
            "You must set a new password to continue.");

        var newPassword = ConsoleHelper.ReadPassword("New Password     ");
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var confirmPassword = ConsoleHelper.ReadPassword("Confirm Password ");
        if (string.IsNullOrWhiteSpace(confirmPassword))
        {
            return false;
        }

        ConsoleHelper.DrawSeparator();
        System.Console.WriteLine("[S] Save and Continue");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option");
        if (!option.Equals("S", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            await _apiClient.PostAsync(
                "api/auth/change-password",
                new ChangePasswordRequest(newPassword, confirmPassword));

            ConsoleHelper.WriteSuccess("Password updated. Welcome!");
            ConsoleHelper.WaitForKey();
            return true;
        }
        catch (ApiException ex)
        {
            ConsoleHelper.WriteError(ex.Message);
            foreach (var error in ex.Errors)
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine($"  - {error}");
                System.Console.ResetColor();
            }
            ConsoleHelper.WaitForKey();
            return false;
        }
    }
}
