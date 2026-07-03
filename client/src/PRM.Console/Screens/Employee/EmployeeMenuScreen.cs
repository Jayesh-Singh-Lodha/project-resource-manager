using PRM.Console.Helpers;
using PRM.Console.Services;

namespace PRM.Console.Screens.Employee;

public class EmployeeMenuScreen
{
    private readonly ApiClient _apiClient;
    private readonly string _fullName;

    public EmployeeMenuScreen(ApiClient apiClient, string fullName)
    {
        _apiClient = apiClient;
        _fullName = fullName;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();

            try
            {
                var response = await _apiClient.GetAsync<Models.Timesheets.TimesheetResponse?>("api/employee/timesheets/last-week-status");
                // 204 No Content will return null if using a standard handler, or it might throw depending on ApiClient implementation.
                // Assuming it returns null for 204.
                if (response == null || response.Status == "Missed")
                {
                    var diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var lastMonday = DateTime.UtcNow.AddDays(-1 * diff).AddDays(-7).Date;
                    System.Console.WriteLine($"\n  ⚠  Reminder: Timesheet for week {lastMonday:dd-MM-yyyy} has not been submitted.\n");
                }
            }
            catch { /* Ignore errors for the reminder banner */ }

            ConsoleHelper.DrawBox(
                $"EMPLOYEE MENU — Welcome, {_fullName}",
                "Select an option below to proceed.");

            System.Console.WriteLine(" [1] Submit Timesheet");
            System.Console.WriteLine(" [2] View Timesheet History");
            System.Console.WriteLine(" [3] My Allocations");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine(" [L] Logout");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option").ToUpper();

            switch (option)
            {
                case "1":
                    await new SubmitTimesheetScreen(_apiClient).ShowAsync();
                    break;
                case "2":
                    await new TimesheetHistoryScreen(_apiClient).ShowAsync();
                    break;
                case "3":
                    await new MyAllocationsScreen(_apiClient).ShowAsync();
                    break;
                case "L":
                    _apiClient.ClearToken();
                    ConsoleHelper.WriteSuccess("Logged out successfully.");
                    ConsoleHelper.WaitForKey();
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option. Please try again.");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }
}
