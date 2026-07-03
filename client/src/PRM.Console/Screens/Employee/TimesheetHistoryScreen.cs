using PRM.Console.Helpers;
using PRM.Console.Models.Timesheets;
using PRM.Console.Services;

namespace PRM.Console.Screens.Employee;

public class TimesheetHistoryScreen
{
    private readonly ApiClient _apiClient;

    public TimesheetHistoryScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("TIMESHEET HISTORY");

            try
            {
                var timesheets = await _apiClient.GetAsync<List<TimesheetResponse>>("api/employee/timesheets");
                var sorted = timesheets.OrderByDescending(x => x.WeekStartDate).ToList();

                System.Console.WriteLine("ID    Week Start    Status      Total Hrs  Submitted At");
                System.Console.WriteLine(new string('─', 65));

                foreach (var t in sorted)
                {
                    var totalHrs = t.Entries.Sum(e => e.HoursWorked);
                    var statusStr = t.Status == "Missed" ? "Missed ⚠" : t.Status;
                    System.Console.WriteLine($"{t.Id,-5} {t.WeekStartDate:yyyy-MM-dd}    {statusStr,-11} {totalHrs,-10} {t.SubmittedAt:yyyy-MM-dd HH:mm}");
                }

                System.Console.WriteLine(new string('─', 65));
                System.Console.WriteLine();
                System.Console.WriteLine("[V] View week details   [B] Back");
                System.Console.WriteLine();

                var option = ConsoleHelper.Prompt("Enter option").ToUpper();

                if (option == "B") return;

                if (option == "V")
                {
                    var tsIdStr = ConsoleHelper.Prompt("Enter Timesheet ID");
                    if (int.TryParse(tsIdStr, out int tsId))
                    {
                        var ts = sorted.FirstOrDefault(x => x.Id == tsId);
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
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to load timesheets: {ex.Message}");
                ConsoleHelper.WaitForKey();
                return;
            }
        }
    }

    private void ShowDetail(TimesheetResponse ts)
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox($"TIMESHEET DETAIL — Week of {ts.WeekStartDate:yyyy-MM-dd}");

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
}
