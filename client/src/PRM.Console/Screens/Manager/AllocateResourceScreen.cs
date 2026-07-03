using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Services;
using System.Text.Json;

namespace PRM.Console.Screens.Manager;

public class AllocateResourceScreen
{
    private readonly ApiClient _apiClient;

    public AllocateResourceScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("ALLOCATE RESOURCE");

        System.Console.WriteLine(" [1] Find resource using AI (recommended)");
        System.Console.WriteLine(" [2] Allocate directly (I already know who I want)");
        System.Console.WriteLine(" [3] End an existing allocation");
        System.Console.WriteLine(" [4] Build a team using AI (multi-role)");
        System.Console.WriteLine(" [B] Back");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option").ToUpper();

        if (option == "B") return;

        if (option == "1")
        {
            var criteria = ConsoleHelper.Prompt("Describe the resource you need (e.g. 'Expert C# developer')");
            try
            {
                var searchResponse = await _apiClient.GetAsync<JsonElement>($"api/manager/allocations/search?criteria={Uri.EscapeDataString(criteria)}");
                var responseText = searchResponse.GetProperty("response").GetString();
                System.Console.WriteLine("\n--- AI Recommendation ---");
                System.Console.WriteLine(responseText);
                System.Console.WriteLine("-------------------------\n");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"AI Search failed: {ex.Message}");
            }
            ConsoleHelper.WaitForKey("Press any key to proceed to allocation...");
        }
        else if (option == "4")
        {
            var criteria = ConsoleHelper.Prompt("Describe the whole team you need (e.g. '1 Senior Java Developer, 1 DevOps Engineer')");
            try
            {
                var searchResponse = await _apiClient.GetAsync<JsonElement>($"api/manager/allocations/build-team?requirements={Uri.EscapeDataString(criteria)}");
                var responseText = searchResponse.GetProperty("response").GetString();
                System.Console.WriteLine("\n--- AI Team Recommendations ---");
                System.Console.WriteLine(responseText);
                System.Console.WriteLine("-------------------------------\n");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"AI Team Builder failed: {ex.Message}");
            }
            ConsoleHelper.WaitForKey("Press any key to proceed to allocation...");
        }
        
        if (option == "1" || option == "2" || option == "4")
        {
            await PerformDirectAllocationAsync();
        }
        else if (option == "3")
        {
            await EndExistingAllocationAsync();
        }
        
        ConsoleHelper.WaitForKey();
    }

    private async Task PerformDirectAllocationAsync()
    {
        var projIdStr = ConsoleHelper.Prompt("Select Project (enter Project ID)");
        if (!int.TryParse(projIdStr, out int projId)) return;

        var empIdStr = ConsoleHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out int empId)) return;

        var pctStr = ConsoleHelper.Prompt("Utilisation Percentage (e.g. 50)");
        if (!int.TryParse(pctStr, out int pct)) return;

        var sdStr = ConsoleHelper.Prompt("From Date (yyyy-mm-dd)");
        DateTime sd = DateTime.TryParse(sdStr, out var parsedSd) ? parsedSd : DateTime.Now;

        var edStr = ConsoleHelper.Prompt("To Date (yyyy-mm-dd)");
        DateTime ed = DateTime.TryParse(edStr, out var parsedEd) ? parsedEd : DateTime.Now.AddMonths(3);

        var request = new CreateAllocationRequest(empId, projId, pct, sd, ed);

        try
        {
            await _apiClient.PostAsync("api/manager/allocations", request);
            ConsoleHelper.WriteSuccess("Resource allocated successfully.");
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to allocate resource: {ex.Message}");
        }
    }

    private async Task EndExistingAllocationAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("END ALLOCATION");

        var projIdStr = ConsoleHelper.Prompt("Select Project (enter Project ID)");
        if (!int.TryParse(projIdStr, out int projId)) return;

        try
        {
            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>(
                $"api/manager/projects/{projId}/allocations");

            var active = allocations.Where(a => a.ToDate >= DateTime.Now.Date).ToList();

            if (!active.Any())
            {
                ConsoleHelper.WriteWarning("No active allocations on this project.");
                return;
            }

            System.Console.WriteLine("\nActive Allocations on this project:");
            System.Console.WriteLine("  #   Employee             %     From         To");
            ConsoleHelper.DrawSeparator();

            for (int i = 0; i < active.Count; i++)
            {
                var a = active[i];
                System.Console.WriteLine($"  {i + 1}.  {a.UserName,-20} {a.UtilisationPercent}%    {a.FromDate:dd-MMM-yy}    {a.ToDate:dd-MMM-yy}");
            }
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();

            var idxStr = ConsoleHelper.Prompt("Select allocation to end (enter #)");
            if (!int.TryParse(idxStr, out int idx) || idx < 1 || idx > active.Count)
            {
                ConsoleHelper.WriteError("Invalid selection.");
                return;
            }

            var selected = active[idx - 1];
            System.Console.WriteLine($"\nEnd {selected.UserName}'s allocation on this project?");
            System.Console.WriteLine($"Set end date to today ({DateTime.Now:dd-MMM-yyyy})?");
            System.Console.WriteLine();
            System.Console.WriteLine("[Y] Yes, End Now    [B] Back");

            var confirm = ConsoleHelper.Prompt("Enter option").ToUpper();
            if (confirm == "Y")
            {
                await _apiClient.DeleteAsync($"api/allocations/{selected.Id}");
                ConsoleHelper.WriteSuccess($"Allocation ended. {selected.UserName} freed from project as of {DateTime.Now:dd-MMM-yyyy}.");
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to end allocation: {ex.Message}");
        }
    }
}
