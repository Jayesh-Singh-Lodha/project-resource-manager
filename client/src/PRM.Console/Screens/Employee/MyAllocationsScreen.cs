using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Services;

namespace PRM.Console.Screens.Employee;

public class MyAllocationsScreen
{
    private readonly ApiClient _apiClient;

    public MyAllocationsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("MY ALLOCATIONS");

        try
        {
            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>("api/employee/allocations");

            System.Console.WriteLine("ID    Project Name         % Util    From Date    To Date      Status");
            System.Console.WriteLine(new string('─', 75));

            foreach (var a in allocations)
            {
                var statusStr = a.ToDate >= DateTime.Now ? "Active" : "Ended";
                System.Console.WriteLine($"{a.Id,-5} {a.ProjectName,-20} {a.UtilisationPercent,-9} {a.FromDate:yyyy-MM-dd}   {a.ToDate:yyyy-MM-dd}   {statusStr}");
            }

            System.Console.WriteLine(new string('─', 75));

            var activeUtil = allocations.Where(a => a.ToDate >= DateTime.Now).Sum(a => a.UtilisationPercent);
            System.Console.WriteLine($"Total Active Utilisation: {activeUtil}%");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load allocations: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
