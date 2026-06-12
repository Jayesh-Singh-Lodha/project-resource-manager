using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ViewAllEmployeesScreen
{
    private readonly ApiClient _apiClient;

    public ViewAllEmployeesScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ALL EMPLOYEES");

        try
        {
            // Employees are users with Role = Employee
            var users = await _apiClient.GetAsync<List<UserResponse>>("api/admin/users");
            var employees = users.Where(u => u.Role == "Employee").ToList();

            System.Console.WriteLine("ID    Name              Dept        Manager ID    Status");
            System.Console.WriteLine(new string('─', 60));

            foreach (var emp in employees)
            {
                var mgrStr = emp.ManagerId?.ToString() ?? "None";
                System.Console.WriteLine($"{emp.Id,-5} {emp.FullName,-17} {emp.Department ?? "N/A",-11} {mgrStr,-13} {(emp.IsActive ? "Active" : "Inactive")}");
            }

            System.Console.WriteLine(new string('─', 60));
            System.Console.WriteLine($"Total Employees: {employees.Count}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load employees: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
