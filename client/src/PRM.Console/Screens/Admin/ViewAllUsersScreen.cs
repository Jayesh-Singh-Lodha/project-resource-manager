using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ViewAllUsersScreen
{
    private readonly ApiClient _apiClient;

    public ViewAllUsersScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ALL USERS");

        try
        {
            var users = await _apiClient.GetAsync<List<UserResponse>>("api/admin/users");

            System.Console.WriteLine("ID    Username          Role        Status");
            System.Console.WriteLine(new string('─', 46));

            int activeCount = 0;
            int inactiveCount = 0;

            foreach (var user in users)
            {
                if (user.IsActive) activeCount++;
                else inactiveCount++;

                var statusStr = user.IsActive ? "Active" : "Inactive";
                System.Console.WriteLine($"{user.Id,-5} {user.Username,-17} {user.Role,-11} {statusStr}");
            }

            System.Console.WriteLine(new string('─', 46));
            System.Console.WriteLine($"Total: {users.Count}   |   Active: {activeCount}   |   Inactive: {inactiveCount}");
            System.Console.WriteLine();
            System.Console.WriteLine("[R] Reactivate a user     [B] Back");
            System.Console.WriteLine();

            while (true)
            {
                var input = ConsoleHelper.Prompt("Enter option").ToUpper();

                if (input == "B")
                {
                    return;
                }
                else if (input == "R")
                {
                    var idStr = ConsoleHelper.Prompt("Enter User ID to reactivate");
                    if (int.TryParse(idStr, out int userId))
                    {
                        var targetUser = users.FirstOrDefault(u => u.Id == userId);
                        if (targetUser != null && !targetUser.IsActive)
                        {
                            System.Console.WriteLine($"\nUser: {targetUser.FullName} ({targetUser.Role}) — currently Inactive");
                            var confirm = ConsoleHelper.Prompt("Reactivate this account? [Y] Yes [B] Cancel").ToUpper();
                            
                            if (confirm == "Y")
                            {
                                await _apiClient.PostAsync($"api/admin/users/{userId}/reactivate", new { });
                                ConsoleHelper.WriteSuccess($"Account reactivated. {targetUser.FullName} can now log in.");
                                ConsoleHelper.WaitForKey();
                                return; // return to refresh or menu
                            }
                        }
                        else
                        {
                            ConsoleHelper.WriteError("User not found or is already active.");
                        }
                    }
                }
                else
                {
                    ConsoleHelper.WriteError("Invalid option.");
                }
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load users: {ex.Message}");
            ConsoleHelper.WaitForKey();
        }
    }
}
