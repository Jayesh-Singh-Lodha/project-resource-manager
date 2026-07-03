using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class UpdateEmployeeScreen
{
    private readonly ApiClient _apiClient;

    public UpdateEmployeeScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("UPDATE EMPLOYEE");

        var idStr = ConsoleHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(idStr, out int userId)) return;

        var newName = ConsoleHelper.Prompt("Enter New Name (leave blank to keep current)");
        var newDept = ConsoleHelper.Prompt("Enter New Department (leave blank to keep current)");
        var newRoleStr = ConsoleHelper.Prompt("Enter Role ID (e.g., 2 for Employee, leave blank to keep current)");

        // Since it's a simple console, we won't fetch current values to fill in blanks here, 
        // we'll assume the API handles partial updates or requires full updates. 
        // Based on the backend, UpdateUserRequest requires FullName, Department, RoleId.
        // Let's fetch the user first to get current values.
        try
        {
            var users = await _apiClient.GetAsync<List<UserResponse>>("api/admin/users");
            var user = users.FirstOrDefault(u => u.Id == userId);
            
            if (user == null)
            {
                ConsoleHelper.WriteError("User not found.");
                ConsoleHelper.WaitForKey();
                return;
            }

            var request = new UpdateUserRequest(
                string.IsNullOrWhiteSpace(newName) ? user.FullName : newName,
                string.IsNullOrWhiteSpace(newDept) ? user.Department : newDept,
                string.IsNullOrWhiteSpace(newRoleStr) ? user.Role : (newRoleStr == "1" ? "Admin" : (newRoleStr == "3" ? "Manager" : "Employee"))
            );

            await _apiClient.PutAsync($"api/admin/users/{userId}", request);
            // ApiClient doesn't have PutAsync! We'll need to use PostAsync or add PutAsync to ApiClient.
            // Oh, ApiClient doesn't have PutAsync. Let's add it!
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Update failed: {ex.Message}");
        }
        ConsoleHelper.WaitForKey();
    }
}
