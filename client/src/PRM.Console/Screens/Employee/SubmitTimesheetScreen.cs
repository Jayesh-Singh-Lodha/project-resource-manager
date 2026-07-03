using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Models.Timesheets;
using PRM.Console.Services;

namespace PRM.Console.Screens.Employee;

/// <summary>
/// Submit Timesheet (BRD Screen 5.1).
/// Auto-loads allocations, uses a numbered tag list, and requires summary confirmation.
/// </summary>
public class SubmitTimesheetScreen
{
    private readonly ApiClient _apiClient;

    public SubmitTimesheetScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("SUBMIT TIMESHEET");

        var dateStr = ConsoleHelper.Prompt("Enter Week Start Date (yyyy-mm-dd) or leave blank for current week");
        var weekStart = DateTime.TryParse(dateStr, out DateTime d) ? d : GetCurrentWeekStart();

        try
        {
            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>("api/employee/allocations");
            var activeAllocations = allocations.Where(a => a.FromDate <= weekStart.AddDays(6) && a.ToDate >= weekStart).ToList();

            if (!activeAllocations.Any())
            {
                ConsoleHelper.WriteWarning("No active allocations for this week. Cannot submit timesheet.");
                ConsoleHelper.WaitForKey();
                return;
            }

            var entries = new List<TimesheetEntryDto>();

            System.Console.WriteLine($"\n--- Add Entry for Week of {weekStart:yyyy-MM-dd} ---");

            for (int i = 0; i < activeAllocations.Count; i++)
            {
                var alloc = activeAllocations[i];
                System.Console.WriteLine($"\nPROJECT {i + 1} OF {activeAllocations.Count} — {alloc.ProjectName}");
                System.Console.WriteLine($"Allocation: {alloc.UtilisationPercent}%");

                var hoursStr = ConsoleHelper.Prompt("Hours worked this week");
                if (!decimal.TryParse(hoursStr, out decimal hours)) 
                {
                    hours = 0;
                }

                if (hours > 0)
                {
                    System.Console.WriteLine("\nActivity Tags:");
                    System.Console.WriteLine(" 1. Coding           5. Deployment        9. Training");
                    System.Console.WriteLine(" 2. Design           6. Bug Fixing       10. Documentation");
                    System.Console.WriteLine(" 3. Testing          7. Code Review      11. Other");
                    System.Console.WriteLine(" 4. Meetings         8. Planning");
                    
                    var tagsInput = ConsoleHelper.Prompt("Select tags (comma-separated numbers)");
                    var tagNames = MapTags(tagsInput);

                    entries.Add(new TimesheetEntryDto(alloc.ProjectId, hours, tagNames));
                }
            }

            if (!entries.Any())
            {
                ConsoleHelper.WriteWarning("No hours entered. Timesheet not submitted.");
                ConsoleHelper.WaitForKey();
                return;
            }

            // SUMMARY
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox($"TIMESHEET SUMMARY — Week of {weekStart:yyyy-MM-dd}");
            
            System.Console.WriteLine("Project ID    Hours    Tags");
            ConsoleHelper.DrawSeparator();
            foreach (var e in entries)
            {
                System.Console.WriteLine($"{e.ProjectId,-13} {e.HoursWorked,-8} {e.ActivityTags}");
            }
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine($"Total Hours: {entries.Sum(e => e.HoursWorked)}\n");

            System.Console.WriteLine("[S] Submit Timesheet   [B] Back/Cancel");
            var option = ConsoleHelper.Prompt("Enter option").ToUpper();

            if (option == "S")
            {
                var request = new SubmitTimesheetRequest(0, weekStart, entries);
                await _apiClient.PostAsync("api/employee/timesheets", request);
                ConsoleHelper.WriteSuccess("Timesheet submitted successfully.");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to submit timesheet: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }

    private DateTime GetCurrentWeekStart()
    {
        var diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
        return DateTime.UtcNow.AddDays(-1 * diff).Date;
    }

    private string MapTags(string input)
    {
        var tags = new Dictionary<string, string>
        {
            { "1", "Coding" }, { "2", "Design" }, { "3", "Testing" }, { "4", "Meetings" },
            { "5", "Deployment" }, { "6", "Bug Fixing" }, { "7", "Code Review" }, { "8", "Planning" },
            { "9", "Training" }, { "10", "Documentation" }, { "11", "Other" }
        };

        var selected = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var result = new List<string>();
        foreach (var s in selected)
        {
            if (tags.TryGetValue(s, out var name)) result.Add(name);
        }
        return string.Join(", ", result);
    }
}
