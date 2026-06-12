using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// Deactivate Employee (BRD Screen 3.1.2).
/// Prompts for ID, shows employee details and active allocations, 
/// and requires a confirmation before deactivating.
/// </summary>
public class DeactivateEmployeeScreen
{
    private readonly ApiClient _apiClient;

    public DeactivateEmployeeScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("DEACTIVATE EMPLOYEE");

        var empIdStr = ConsoleHelper.Prompt("Enter Employee ID to deactivate");
        if (!int.TryParse(empIdStr, out int empId)) return;

        try
        {
            var users = await _apiClient.GetAsync<List<UserResponse>>("api/admin/users");
            var emp = users.FirstOrDefault(u => u.Id == empId && u.Role == "Employee");

            if (emp == null)
            {
                ConsoleHelper.WriteError("Employee not found or user is not an employee.");
                ConsoleHelper.WaitForKey();
                return;
            }

            if (!emp.IsActive)
            {
                ConsoleHelper.WriteWarning("Employee is already deactivated.");
                ConsoleHelper.WaitForKey();
                return;
            }

            System.Console.WriteLine($"\nEmployee Details:");
            System.Console.WriteLine($"Name       : {emp.FullName}");
            System.Console.WriteLine($"Department : {emp.Department ?? "N/A"}");
            System.Console.WriteLine($"Status     : {emp.Status}");
            System.Console.WriteLine();

            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>("api/allocations");
            var activeAlloc = allocations.Where(a => a.UserId == emp.Id && a.ToDate >= DateTime.Now.Date).ToList();

            if (activeAlloc.Any())
            {
                ConsoleHelper.WriteWarning($"This employee has {activeAlloc.Count} active allocations.");
                System.Console.WriteLine("These allocations will be automatically ended if you proceed:");
                foreach (var a in activeAlloc)
                {
                    System.Console.WriteLine($"  - {a.ProjectName} ({a.UtilisationPercent}%) until {a.ToDate:dd-MMM-yyyy}");
                }
                System.Console.WriteLine();
            }
            else
            {
                System.Console.WriteLine("This employee currently has no active allocations.\n");
            }

            System.Console.WriteLine("[Y] Yes, Deactivate   [B] Cancel");
            var option = ConsoleHelper.Prompt("Enter option").ToUpper();

            if (option == "Y")
            {
                await _apiClient.PostAsync($"api/admin/users/{emp.Id}/deactivate", new { });
                ConsoleHelper.WriteSuccess("Employee deactivated successfully.");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to deactivate employee: {ex.Message}");
        }

        ConsoleHelper.WaitForKey();
    }
}
