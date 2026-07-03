using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// Create User screen — Admin creates a new user account.
/// Prompts for username, email, full name, department, and role.
/// Displays the generated temporary password once on success.
/// </summary>
public class CreateUserScreen
{
    private readonly ApiClient _apiClient;

    public CreateUserScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    /// <summary>
    /// Shows the create user form and calls the API.
    /// </summary>
    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox(
            "CREATE USER ACCOUNT",
            "Fill in the details below.");

        var username = ConsoleHelper.Prompt("Username");
        if (string.IsNullOrWhiteSpace(username))
        {
            ConsoleHelper.WriteError("Username cannot be empty.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var email = ConsoleHelper.Prompt("Email");
        if (string.IsNullOrWhiteSpace(email))
        {
            ConsoleHelper.WriteError("Email cannot be empty.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var fullName = ConsoleHelper.Prompt("Full Name");
        if (string.IsNullOrWhiteSpace(fullName))
        {
            ConsoleHelper.WriteError("Full name cannot be empty.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var department = ConsoleHelper.Prompt("Department (optional, press Enter to skip)");
        if (string.IsNullOrWhiteSpace(department))
        {
            department = null;
        }

        ConsoleHelper.DrawSeparator();
        System.Console.WriteLine(" Roles: [1] Admin  [2] Manager  [3] Employee");
        var roleOption = ConsoleHelper.Prompt("Select role");

        var role = roleOption switch
        {
            "1" => "Admin",
            "2" => "Manager",
            "3" => "Employee",
            _ => null
        };

        if (role is null)
        {
            ConsoleHelper.WriteError("Invalid role selection.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.DrawSeparator();
        System.Console.WriteLine($" Username  : {username}");
        System.Console.WriteLine($" Email     : {email}");
        System.Console.WriteLine($" Full Name : {fullName}");
        System.Console.WriteLine($" Department: {department ?? "(none)"}");
        System.Console.WriteLine($" Role      : {role}");
        ConsoleHelper.DrawSeparator();

        System.Console.WriteLine(" [C] Confirm and Create");
        System.Console.WriteLine(" [X] Cancel");
        System.Console.WriteLine();

        var confirm = ConsoleHelper.Prompt("Enter option").ToUpper();
        if (confirm != "C")
        {
            ConsoleHelper.WriteWarning("User creation cancelled.");
            ConsoleHelper.WaitForKey();
            return;
        }

        try
        {
            var request = new CreateUserRequest(username, email, fullName, role, department);
            var response = await _apiClient.PostAsync<CreateUserRequest, CreateUserResponse>(
                "api/admin/users", request);

            ConsoleHelper.WriteSuccess($"User '{response.Username}' created successfully!");
            System.Console.WriteLine();

            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine("╔══════════════════════════════════════════════╗");
            System.Console.WriteLine("║  TEMPORARY PASSWORD (shown once only!)       ║");
            System.Console.WriteLine($"║  {response.TemporaryPassword.PadRight(43)}║");
            System.Console.WriteLine("║  The user MUST change this on first login.   ║");
            System.Console.WriteLine("╚══════════════════════════════════════════════╝");
            System.Console.ResetColor();

            ConsoleHelper.WaitForKey();
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
        }
        catch (HttpRequestException)
        {
            ConsoleHelper.WriteError("Cannot connect to the server. Is the API running?");
            ConsoleHelper.WaitForKey();
        }
    }
}
