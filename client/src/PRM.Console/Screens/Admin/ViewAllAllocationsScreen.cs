using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ViewAllAllocationsScreen
{
    private readonly ApiClient _apiClient;

    public ViewAllAllocationsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ALL ALLOCATIONS");

        try
        {
            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>("api/allocations");

            System.Console.WriteLine("ID    Project ID    Employee ID   Percentage    Status");
            System.Console.WriteLine(new string('─', 60));

            foreach (var a in allocations)
            {
                var statusStr = a.ToDate >= DateTime.Now ? "Active" : "Ended";
                System.Console.WriteLine($"{a.Id,-5} {a.ProjectId,-13} {a.UserId,-13} {a.UtilisationPercent,-13} {statusStr}");
            }

            System.Console.WriteLine(new string('─', 60));
            System.Console.WriteLine($"Total Allocations: {allocations.Count}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load allocations: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
