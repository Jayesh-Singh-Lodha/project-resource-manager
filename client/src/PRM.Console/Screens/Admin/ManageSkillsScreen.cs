using PRM.Console.Helpers;
using PRM.Console.Models.Users;
using PRM.Console.Services;

namespace PRM.Console.Screens.Admin;

public class ManageSkillsScreen
{
    private readonly ApiClient _apiClient;

    public ManageSkillsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("MANAGE EMPLOYEE SKILLS");

        var empIdStr = ConsoleHelper.Prompt("Enter Employee ID");
        if (!int.TryParse(empIdStr, out int empId)) return;

        System.Console.WriteLine();
        System.Console.WriteLine(" [1] Add/Update Skill");
        System.Console.WriteLine(" [2] Remove Skill");
        System.Console.WriteLine(" [B] Back");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option").ToUpper();

        if (option == "1")
        {
            var skillName = ConsoleHelper.Prompt("Skill Name (e.g., C#)");
            var proficiency = ConsoleHelper.Prompt("Proficiency (Beginner, Intermediate, Expert)");

            var request = new AddSkillRequest(skillName, proficiency, "");
            
            try
            {
                await _apiClient.PostAsync($"api/admin/users/{empId}/skills", request);
                ConsoleHelper.WriteSuccess("Skill added/updated successfully.");
            }
            catch (Exception ex)
            {
                // If adding fails, try updating
                try
                {
                    var updateRequest = new UpdateSkillRequest(skillName, proficiency);
                    await _apiClient.PutAsync($"api/admin/users/{empId}/skills", updateRequest);
                    ConsoleHelper.WriteSuccess("Skill updated successfully.");
                }
                catch
                {
                    ConsoleHelper.WriteError($"Failed to manage skill: {ex.Message}");
                }
            }
        }
        else if (option == "2")
        {
            var skillName = ConsoleHelper.Prompt("Skill Name to remove");
            try
            {
                await _apiClient.DeleteAsync($"api/admin/users/{empId}/skills/{skillName}");
                ConsoleHelper.WriteSuccess("Skill removed successfully.");
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Failed to remove skill: {ex.Message}");
            }
        }
        
        ConsoleHelper.WaitForKey();
    }
}
