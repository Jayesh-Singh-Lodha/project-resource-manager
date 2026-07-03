using PRM.Console.Helpers;
using PRM.Console.Models.Auth;
using PRM.Console.Services;

namespace PRM.Console.Screens;

/// <summary>
/// Login screen — BRD Screen 1, Option 1.
/// Prompts for username and password, authenticates via API.
/// On success, stores the JWT token in ApiClient and returns the user's role.
/// If ForcePasswordChange is true, redirects to ChangePasswordScreen first.
/// </summary>
public class LoginScreen
{
    private readonly ApiClient _apiClient;

    public LoginScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Shows the login prompt and authenticates the user.
    /// Returns the authenticated user's role, or null if login was cancelled.
    /// </summary>
    public async Task<(string Role, string FullName)?> ShowAsync()
    {
        ConsoleHelper.DrawBox(
            "LOGIN",
            "Enter your credentials below.");

        var username = ConsoleHelper.Prompt("Username");
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        var password = ConsoleHelper.ReadPassword("Password");
        if (string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        System.Console.WriteLine();
        System.Console.Write("Authenticating...");

        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>(
                "api/auth/login",
                new LoginRequest(username, password));

            // Store the JWT token
            _apiClient.Token = response.Token;

            System.Console.WriteLine(" done.");

            // Force password change on first login
            if (response.ForcePasswordChange)
            {
                var changePasswordScreen = new ChangePasswordScreen(_apiClient);
                var changed = await changePasswordScreen.ShowAsync();

                if (!changed)
                {
                    _apiClient.ClearToken();
                    ConsoleHelper.WriteError("Password change is required. Cannot continue.");
                    ConsoleHelper.WaitForKey();
                    return null;
                }
            }

            return (response.Role, response.FullName);
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
            return null;
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteError("Cannot connect to the server. Is the API running?");
            ConsoleHelper.WaitForKey();
            return null;
        }
    }
}
