using PRM.Console.Helpers;
using PRM.Console.Models.Projects;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ManageMilestonesScreen
{
    private readonly ApiClient _apiClient;

    public ManageMilestonesScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("MANAGE MILESTONES");

        var idStr = ConsoleHelper.Prompt("Enter Project ID");
        if (!int.TryParse(idStr, out int projectId)) return;

        System.Console.WriteLine();
        System.Console.WriteLine(" [1] Add Milestone");
        System.Console.WriteLine(" [2] Update Milestone Status");
        System.Console.WriteLine(" [B] Back");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option").ToUpper();

        if (option == "1")
        {
            var title = ConsoleHelper.Prompt("Milestone Title");
            var targetDateStr = ConsoleHelper.Prompt("Target Date (yyyy-mm-dd)");
            if (!DateTime.TryParse(targetDateStr, out DateTime targetDate)) return;

            var request = new AddMilestoneRequest(title, targetDate, 0);
            
            try
            {
                await _apiClient.PostAsync($"api/projects/{projectId}/milestones", request);
                ConsoleHelper.WriteSuccess("Milestone added successfully.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to add milestone: {ex.Message}");
            }
        }
        else if (option == "2")
        {
            var milestoneIdStr = ConsoleHelper.Prompt("Milestone ID");
            if (!int.TryParse(milestoneIdStr, out int mId)) return;

            var status = ConsoleHelper.Prompt("Status (Pending, Completed, Delayed)");
            var request = new UpdateMilestoneStatusRequest(status);

            try
            {
                await _apiClient.PutAsync($"api/projects/milestones/{mId}/status", request);
                ConsoleHelper.WriteSuccess("Milestone status updated successfully.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to update milestone: {ex.Message}");
            }
        }
        
        ConsoleHelper.WaitForKey();
    }
}
