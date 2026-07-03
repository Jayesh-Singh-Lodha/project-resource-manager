using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

/// <summary>
/// View All Allocations (BRD Screen 3.2.1).
/// Lists all allocations with filtering by project or employee name.
/// </summary>
public class ViewAllocationsScreen
{
    private readonly ApiClient _apiClient;

    public ViewAllocationsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        string? filterStr = null;

        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("ALL ALLOCATIONS");

            try
            {
                var allocations = await _apiClient.GetAsync<List<AllocationResponse>>("api/allocations");

                if (!string.IsNullOrWhiteSpace(filterStr))
                {
                    allocations = allocations.Where(a => 
                        a.ProjectName.Contains(filterStr, StringComparison.OrdinalIgnoreCase) ||
                        a.UserName.Contains(filterStr, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                    System.Console.WriteLine($"Filter active: '{filterStr}' (Project/Employee)\n");
                }

                System.Console.WriteLine("ID    Project Name         Employee Name        % Util  From Date    To Date");
                System.Console.WriteLine(new string('─', 85));

                foreach (var a in allocations)
                {
                    System.Console.WriteLine($"{a.Id,-5} {a.ProjectName,-20} {a.UserName,-20} {a.UtilisationPercent,-7} {a.FromDate:yyyy-MM-dd}   {a.ToDate:yyyy-MM-dd}");
                }

                System.Console.WriteLine(new string('─', 85));
                System.Console.WriteLine($"Total: {allocations.Count}");
                System.Console.WriteLine();

                System.Console.WriteLine("[F] Filter    [C] Clear Filter    [B] Back");
                var option = ConsoleHelper.Prompt("Enter option").ToUpper();

                if (option == "B") return;

                if (option == "F")
                {
                    filterStr = ConsoleHelper.Prompt("Enter Project or Employee name to filter by");
                }
                else if (option == "C")
                {
                    filterStr = null;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to load allocations: {ex.Message}");
                ConsoleHelper.WaitForKey();
                return;
            }
        }
    }
}
