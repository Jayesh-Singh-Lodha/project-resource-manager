using PRM.Console.Helpers;
using PRM.Console.Models.Projects;
using PRM.Console.Services;
using System.Text.Json;

namespace PRM.Console.Screens.Manager;

/// <summary>
/// AI Assistant screen (BRD Screen 4.5).
/// Provides Skill Match and Risk Summary features via the LLM.
/// </summary>
public class AiAssistantScreen
{
    private readonly ApiClient _apiClient;

    public AiAssistantScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DrawBox("AI ASSISTANT");

            System.Console.WriteLine(" [1] Skill Match   — Find best employees for a project requirement");
            System.Console.WriteLine(" [2] Risk Summary  — Get a health analysis for a project");
            System.Console.WriteLine(" [3] Team Builder  — Define a whole team at once and find best available resources");
            System.Console.WriteLine(" [B] Back");
            System.Console.WriteLine();

            var option = ConsoleHelper.Prompt("Enter option").ToUpper();

            switch (option)
            {
                case "1":
                    await ShowSkillMatchAsync();
                    break;
                case "2":
                    await ShowRiskSummaryAsync();
                    break;
                case "3":
                    await ShowTeamBuilderAsync();
                    break;
                case "B":
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option.");
                    ConsoleHelper.WaitForKey();
                    break;
            }
        }
    }

    private async Task ShowSkillMatchAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("SKILL MATCH");

        var criteria = ConsoleHelper.Prompt("Describe your project requirement in plain English");

        if (string.IsNullOrWhiteSpace(criteria))
        {
            ConsoleHelper.WriteWarning("No criteria entered.");
            ConsoleHelper.WaitForKey();
            return;
        }

        System.Console.WriteLine("\nSearching... (calling AI)\n");

        try
        {
            var searchResponse = await _apiClient.GetAsync<JsonElement>(
                $"api/manager/allocations/search?criteria={Uri.EscapeDataString(criteria)}");
            var responseText = searchResponse.GetProperty("response").GetString();

            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine("AI-MATCHED RESULTS");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine(responseText);
            System.Console.WriteLine();
            System.Console.WriteLine("  Note: These are AI-generated suggestions. Always verify availability");
            System.Console.WriteLine("  and skills with the employee before allocating.");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();
            System.Console.WriteLine("[A] Go to Allocate Resource     [B] Back");
            System.Console.WriteLine();

            var next = ConsoleHelper.Prompt("Enter option").ToUpper();
            if (next == "A")
            {
                await new AllocateResourceScreen(_apiClient).ShowAsync();
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"AI search failed: {ex.Message}");
            ConsoleHelper.WaitForKey();
        }
    }

    private async Task ShowRiskSummaryAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("RISK SUMMARY");

        try
        {
            var projects = await _apiClient.GetAsync<List<ProjectResponse>>("api/manager/projects");

            if (!projects.Any())
            {
                ConsoleHelper.WriteWarning("No projects found.");
                ConsoleHelper.WaitForKey();
                return;
            }

            System.Console.WriteLine("Select project:");
            for (int i = 0; i < projects.Count; i++)
            {
                var healthEmoji = projects[i].HealthStatus switch
                {
                    "AtRisk" => "🔴 AT RISK",
                    "Attention" => "🟡 ATTENTION",
                    "OnTrack" => "🟢 ON TRACK",
                    _ => projects[i].HealthStatus
                };
                System.Console.WriteLine($"  {i + 1}.  {projects[i].Name,-20} {healthEmoji}");
            }
            System.Console.WriteLine();

            var idxStr = ConsoleHelper.Prompt("Enter project number");
            if (!int.TryParse(idxStr, out int idx) || idx < 1 || idx > projects.Count)
            {
                ConsoleHelper.WriteError("Invalid selection.");
                ConsoleHelper.WaitForKey();
                return;
            }

            var selected = projects[idx - 1];
            System.Console.WriteLine($"\nGenerating AI summary for {selected.Name}...\n");

            var response = await _apiClient.GetAsync<JsonElement>($"api/manager/projects/{selected.Id}/risk-summary");
            var summary = response.GetProperty("summary").GetString();

            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine($"AI Risk Summary — {selected.Name}");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();
            System.Console.WriteLine($"\"{summary}\"");
            System.Console.WriteLine();
            System.Console.WriteLine("  Note: AI-generated from current milestone and timesheet data.");
            ConsoleHelper.DrawSeparator();
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Risk summary failed: {ex.Message}");
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task ShowTeamBuilderAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("TEAM BUILDER");

        var requirements = ConsoleHelper.Prompt("Describe the whole team you need (e.g. '1 Senior Java Developer, 1 DevOps Engineer')");

        if (string.IsNullOrWhiteSpace(requirements))
        {
            ConsoleHelper.WriteWarning("No requirements entered.");
            ConsoleHelper.WaitForKey();
            return;
        }

        System.Console.WriteLine("\nSearching... (calling AI)\n");

        try
        {
            var searchResponse = await _apiClient.GetAsync<JsonElement>(
                $"api/manager/allocations/build-team?requirements={Uri.EscapeDataString(requirements)}");
            var responseText = searchResponse.GetProperty("response").GetString();

            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine("AI TEAM RECOMMENDATIONS");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine(responseText);
            System.Console.WriteLine();
            System.Console.WriteLine("  Note: These are AI-generated suggestions. Always verify availability");
            System.Console.WriteLine("  and skills with the employee before allocating.");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"AI Team Builder failed: {ex.Message}");
        }
        
        ConsoleHelper.WaitForKey();
    }
}
