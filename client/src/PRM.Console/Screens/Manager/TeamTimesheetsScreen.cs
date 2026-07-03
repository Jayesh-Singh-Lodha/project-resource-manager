using PRM.Console.Helpers;
using PRM.Console.Models.Timesheets;
using PRM.Console.Services;

namespace PRM.Console.Screens.Manager;

/// <summary>
/// Team Timesheets (BRD Screen 4.4).
/// Shows team timesheets for a week, flags Missed, and allows drill-down before approval.
/// </summary>
public class TeamTimesheetsScreen
{
    private readonly ApiClient _apiClient;

    public TeamTimesheetsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("TEAM TIMESHEETS");

            var dateStr = ConsoleHelper.Prompt("Enter Week Start Date (yyyy-mm-dd) or leave blank for current week");
            var weekStart = DateTime.TryParse(dateStr, out DateTime d) ? d : GetCurrentWeekStart();

            try
            {
                var timesheets = await _apiClient.GetAsync<List<TimesheetResponse>>($"api/manager/timesheets?weekStartDate={weekStart:yyyy-MM-dd}");

                System.Console.WriteLine($"Week of {weekStart:yyyy-MM-dd}");
                System.Console.WriteLine("ID    Employee          Status      Total Hrs");
                System.Console.WriteLine(new string('─', 50));

                foreach (var t in timesheets)
                {
                    var totalHrs = t.Entries.Sum(e => e.HoursWorked);
                    var statusStr = t.Status == "Missed" ? "Missed ⚠" : t.Status;
                    System.Console.WriteLine($"{t.Id,-5} {t.UserName,-17} {statusStr,-11} {totalHrs}");
                }

                System.Console.WriteLine(new string('─', 50));
                System.Console.WriteLine();
                System.Console.WriteLine("[V] View details   [A] Approve/Reject   [B] Back");
                
                var option = ConsoleHelper.Prompt("\nEnter option").ToUpper();
                if (option == "B") return;

                if (option == "V")
                {
                    var tsIdStr = ConsoleHelper.Prompt("Timesheet ID");
                    if (int.TryParse(tsIdStr, out int tsId))
                    {
                        var ts = timesheets.FirstOrDefault(x => x.Id == tsId);
                        if (ts != null)
                        {
                            ShowDetail(ts);
                        }
                        else
                        {
                            ConsoleHelper.WriteError("Timesheet not found.");
                            ConsoleHelper.WaitForKey();
                        }
                    }
                }
                else if (option == "A")
                {
                    var tsIdStr = ConsoleHelper.Prompt("Timesheet ID");
                    if (int.TryParse(tsIdStr, out int tsId))
                    {
                        var status = ConsoleHelper.Prompt("Status (Approved, Rejected)");
                        var jsonStatus = $"\"{status}\""; 
                        await _apiClient.PutAsync($"api/manager/timesheets/{tsId}/status", jsonStatus);
                        ConsoleHelper.WriteSuccess("Timesheet status updated.");
                        ConsoleHelper.WaitForKey();
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"An error occurred: {ex.Message}");
                ConsoleHelper.WaitForKey();
            }
        }
    }

    private void ShowDetail(TimesheetResponse ts)
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox($"TIMESHEET DETAIL — {ts.UserName}");

        System.Console.WriteLine($"Status       : {(ts.Status == "Missed" ? "Missed ⚠" : ts.Status)}");
        System.Console.WriteLine($"Submitted At : {ts.SubmittedAt:yyyy-MM-dd HH:mm}");
        System.Console.WriteLine();

        if (!ts.Entries.Any())
        {
            System.Console.WriteLine("No entries found for this week.");
        }
        else
        {
            System.Console.WriteLine("Project                        Hours    Tags");
            ConsoleHelper.DrawSeparator();
            foreach (var e in ts.Entries)
            {
                System.Console.WriteLine($"{e.ProjectName,-30} {e.HoursWorked,-8} {e.ActivityTags}");
            }
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine($"Total Hours: {ts.Entries.Sum(e => e.HoursWorked)}\n");
        }

        ConsoleHelper.WaitForKey();
    }

    private DateTime GetCurrentWeekStart()
    {
        var diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
        return DateTime.UtcNow.AddDays(-1 * diff).Date;
    }
}
