using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;
using System.Text.Json;

namespace PRM.Console.Screens.Manager;

/// <summary>
/// Resource Dashboard (BRD Screen 4.1).
/// Shows team grouped by status (Bench/Active), skills, allocation %, and allows drill-down.
/// </summary>
public class ResourceDashboardScreen
{
    private readonly ApiClient _apiClient;

    public ResourceDashboardScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("RESOURCE DASHBOARD", "View team status and bench resources.");

            try
            {
                var team = await _apiClient.GetAsync<List<UserResponse>>("api/manager/team");

                var bench = team.Where(e => e.Status.Equals("Bench", StringComparison.OrdinalIgnoreCase)).ToList();
                var active = team.Where(e => e.Status.Equals("Allocated", StringComparison.OrdinalIgnoreCase)).ToList();

                System.Console.WriteLine("--- ON BENCH (Available immediately) ---");
                System.Console.WriteLine("ID    Name              Department  Skills");
                ConsoleHelper.DrawSeparator();
                
                foreach (var emp in bench)
                {
                    var skillsStr = emp.Skills != null && emp.Skills.Any() 
                        ? string.Join(", ", emp.Skills) 
                        : "None";
                    System.Console.WriteLine($"{emp.Id,-5} {emp.FullName,-17} {emp.Department ?? "N/A",-11} {skillsStr}");
                }
                
                if (!bench.Any()) System.Console.WriteLine("No employees on bench.");
                System.Console.WriteLine();

                System.Console.WriteLine("--- ACTIVE EMPLOYEES ---");
                System.Console.WriteLine("ID    Name              Alloc %     Availability");
                ConsoleHelper.DrawSeparator();

                foreach (var emp in active)
                {
                    var util = emp.CurrentUtilisationPercent;
                    var availStr = util >= 100 ? "FULL" : $"{100 - util}% free";
                    System.Console.WriteLine($"{emp.Id,-5} {emp.FullName,-17} {util,-11} {availStr}");
                }

                if (!active.Any()) System.Console.WriteLine("No active employees.");
                
                ConsoleHelper.DrawSeparator();
                System.Console.WriteLine($"Summary   |   Bench: {bench.Count}   |   Partial/Active: {active.Count}");
                System.Console.WriteLine();

                System.Console.WriteLine("[D] Drill into employee detail   [B] Back");
                System.Console.WriteLine();

                var option = ConsoleHelper.Prompt("Enter option").ToUpper();

                if (option == "B") return;

                if (option == "D")
                {
                    await ShowEmployeeDetailAsync(team);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to load dashboard: {ex.Message}");
                ConsoleHelper.WaitForKey();
                return;
            }
        }
    }

    private async Task ShowEmployeeDetailAsync(List<UserResponse> team)
    {
        var empIdStr = ConsoleHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out int empId)) return;

        var emp = team.FirstOrDefault(e => e.Id == empId);
        if (emp is null)
        {
            ConsoleHelper.WriteError("Employee not found in your team.");
            ConsoleHelper.WaitForKey();
            return;
        }

        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox($"EMPLOYEE DETAIL — {emp.FullName}");

        System.Console.WriteLine($"Department : {emp.Department ?? "N/A"}");
        System.Console.WriteLine($"Status     : {emp.Status}");
        
        var skillsStr = emp.Skills != null && emp.Skills.Any() 
            ? string.Join(", ", emp.Skills) 
            : "None";
        System.Console.WriteLine($"Skills     : {skillsStr}");
        System.Console.WriteLine();

        try
        {
            var detailNode = await _apiClient.GetAsync<JsonElement>($"api/manager/employees/{emp.Id}/detail");
            
            var allocations = detailNode.GetProperty("allocations").EnumerateArray().ToList();
            if (allocations.Any())
            {
                System.Console.WriteLine("Active Allocations:");
                foreach (var a in allocations)
                {
                    var projName = a.GetProperty("projectName").GetString();
                    var pct = a.GetProperty("utilisationPercent").GetInt32();
                    var to = a.GetProperty("toDate").GetDateTime();
                    System.Console.WriteLine($"  - {projName} ({pct}%) until {to:dd-MMM-yyyy}");
                }
            }
            else
            {
                System.Console.WriteLine("Active Allocations: None");
            }
            System.Console.WriteLine();

            var tags = detailNode.GetProperty("recentActivityTags").EnumerateArray().Select(t => t.GetString()).ToList();
            if (tags.Any())
            {
                System.Console.WriteLine("Recent Activity Tags (last 4 weeks):");
                System.Console.WriteLine($"  {string.Join(", ", tags)}");
            }
            else
            {
                System.Console.WriteLine("Recent Activity Tags: None");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load extra details: {ex.Message}");
        }

        System.Console.WriteLine();
        ConsoleHelper.WaitForKey();
    }
}
