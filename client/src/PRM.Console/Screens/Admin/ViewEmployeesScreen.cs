using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// View Employees (BRD Screen 3.1).
/// Lists all employees with role Employee. Supports filtering by department/status.
/// </summary>
public class ViewEmployeesScreen
{
    private readonly ApiClient _apiClient;

    public ViewEmployeesScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        string? filterStr = null;

        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("EMPLOYEE DIRECTORY");

            try
            {
                var users = await _apiClient.GetAsync<List<UserResponse>>("api/admin/users");
                var employees = users.Where(u => u.Role == "Employee").ToList();

                if (!string.IsNullOrWhiteSpace(filterStr))
                {
                    employees = employees.Where(e => 
                        (e.Department != null && e.Department.Contains(filterStr, StringComparison.OrdinalIgnoreCase)) ||
                        e.Status.Contains(filterStr, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                    System.Console.WriteLine($"Filter active: '{filterStr}' (Department/Status)\n");
                }

                System.Console.WriteLine("ID    Name                Department       Status      Active");
                System.Console.WriteLine(new string('─', 70));

                foreach (var emp in employees)
                {
                    var statusStr = emp.IsActive ? emp.Status : "Deactivated";
                    var activeMark = emp.IsActive ? "Yes" : "No";
                    System.Console.WriteLine($"{emp.Id,-5} {emp.FullName,-19} {emp.Department ?? "N/A",-16} {statusStr,-11} {activeMark}");
                }

                System.Console.WriteLine(new string('─', 70));
                System.Console.WriteLine($"Total: {employees.Count}");
                System.Console.WriteLine();

                System.Console.WriteLine("[F] Filter    [C] Clear Filter    [B] Back");
                var option = ConsoleHelper.Prompt("Enter option").ToUpper();

                if (option == "B") return;

                if (option == "F")
                {
                    filterStr = ConsoleHelper.Prompt("Enter Department or Status to filter by");
                }
                else if (option == "C")
                {
                    filterStr = null;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to load employees: {ex.Message}");
                ConsoleHelper.WaitForKey();
                return;
            }
        }
    }
}
