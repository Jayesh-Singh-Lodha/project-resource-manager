using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class AssignManagerScreen
{
    private readonly ApiClient _apiClient;

    public AssignManagerScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ASSIGN MANAGER");

        var empIdStr = ConsoleHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out int empId)) return;

        var mgrIdStr = ConsoleHelper.Prompt("Enter Manager ID (leave blank to remove manager)");
        int? managerId = int.TryParse(mgrIdStr, out int mid) ? mid : null;

        try
        {
            var url = managerId.HasValue ? $"api/admin/users/{empId}/manager?managerId={managerId}" : $"api/admin/users/{empId}/manager";
            await _apiClient.PostAsync(url, new { });
            ConsoleHelper.WriteSuccess("Manager assigned successfully.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to assign manager: {ex.Message}");
        }
        ConsoleHelper.WaitForKey();
    }
}
