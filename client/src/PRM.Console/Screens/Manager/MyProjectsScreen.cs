using PRM.Console.Helpers;
using PRM.Console.Models.Allocations;
using PRM.Console.Models.Projects;
using PRM.Console.Services;
using System.Text.Json;

namespace PRM.Console.Screens.Manager;

/// <summary>
/// My Projects screen (BRD Screen 4.3) with drill-down into project detail,
/// milestones, allocated resources, and AI risk summary.
/// </summary>
public class MyProjectsScreen
{
    private readonly ApiClient _apiClient;

    public MyProjectsScreen(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task ShowAsync()
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox("MY PROJECTS");

        try
        {
            var projects = await _apiClient.GetAsync<List<ProjectResponse>>("api/manager/projects");

            if (!projects.Any())
            {
                ConsoleHelper.WriteWarning("No projects found.");
                ConsoleHelper.WaitForKey();
                return;
            }

            System.Console.WriteLine("#    Project              End Date     Health");
            ConsoleHelper.DrawSeparator();

            for (int i = 0; i < projects.Count; i++)
            {
                var p = projects[i];
                var healthEmoji = MapHealthEmoji(p.HealthStatus);
                System.Console.WriteLine($"{i + 1}.   {p.Name,-20} {p.EndDate:dd-MMM-yy}    {healthEmoji}");
            }
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();

            var input = ConsoleHelper.Prompt("Select project number to view details (or B to go back)").ToUpper();
            if (input == "B") return;

            if (int.TryParse(input, out int idx) && idx >= 1 && idx <= projects.Count)
            {
                await ShowProjectDetailAsync(projects[idx - 1]);
            }
            else
            {
                ConsoleHelper.WriteError("Invalid selection.");
                ConsoleHelper.WaitForKey();
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to load projects: {ex.Message}");
            ConsoleHelper.WaitForKey();
        }
    }

    private async Task ShowProjectDetailAsync(ProjectResponse project)
    {
        ConsoleHelper.ClearScreen();
        ConsoleHelper.DrawBox(project.Name);

        var healthEmoji = MapHealthEmoji(project.HealthStatus);
        System.Console.WriteLine($"Health Status : {healthEmoji}");
        System.Console.WriteLine($"Status       : {project.Status}");
        System.Console.WriteLine($"Start Date   : {project.StartDate:dd-MMM-yyyy}");
        System.Console.WriteLine($"End Date     : {project.EndDate:dd-MMM-yyyy}");
        System.Console.WriteLine($"Story Points : {project.StoryPointsCompleted} / {project.TotalStoryPoints}");
        System.Console.WriteLine();

        // Milestones
        try
        {
            var milestones = await _apiClient.GetAsync<List<MilestoneResponse>>($"api/projects/{project.Id}/milestones");

            if (milestones.Any())
            {
                System.Console.WriteLine("Milestones:");
                System.Console.WriteLine("  #    Title                Due Date     Status");
                ConsoleHelper.DrawSeparator();

                for (int i = 0; i < milestones.Count; i++)
                {
                    var m = milestones[i];
                    var overdue = m.DueDate < DateTime.Now && m.Status != "Done" ? "  ⚠ OVERDUE" : "";
                    System.Console.WriteLine($"  {i + 1}.   {m.Title,-20} {m.DueDate:dd-MMM-yy}    {m.Status}{overdue}");
                }
                ConsoleHelper.DrawSeparator();
                System.Console.WriteLine();
            }
        }
        catch { /* Milestones not critical if they fail */ }

        // Allocated Resources
        try
        {
            var allocations = await _apiClient.GetAsync<List<AllocationResponse>>(
                $"api/manager/projects/{project.Id}/allocations");
            var active = allocations.Where(a => a.ToDate >= DateTime.Now.Date).ToList();

            if (active.Any())
            {
                System.Console.WriteLine("Allocated Resources:");
                System.Console.WriteLine("  Name                 %      From         To");
                ConsoleHelper.DrawSeparator();

                foreach (var a in active)
                {
                    System.Console.WriteLine($"  {a.UserName,-20} {a.UtilisationPercent}%    {a.FromDate:dd-MMM-yy}    {a.ToDate:dd-MMM-yy}");
                }
                ConsoleHelper.DrawSeparator();
                System.Console.WriteLine();
            }
        }
        catch { /* Allocations not critical if they fail */ }

        System.Console.WriteLine("[A] Get AI Risk Summary     [B] Back");
        System.Console.WriteLine();

        var option = ConsoleHelper.Prompt("Enter option").ToUpper();
        if (option == "A")
        {
            await ShowAiRiskSummaryAsync(project);
        }
    }

    private async Task ShowAiRiskSummaryAsync(ProjectResponse project)
    {
        System.Console.WriteLine($"\nGenerating AI risk summary for {project.Name}...\n");

        try
        {
            var response = await _apiClient.GetAsync<JsonElement>($"api/manager/projects/{project.Id}/risk-summary");
            var summary = response.GetProperty("summary").GetString();

            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine($"AI Risk Summary — {project.Name}");
            ConsoleHelper.DrawSeparator();
            System.Console.WriteLine();
            System.Console.WriteLine($"\"{summary}\"");
            System.Console.WriteLine();
            System.Console.WriteLine("  Note: This summary is AI-generated from milestone and timesheet data.");
            ConsoleHelper.DrawSeparator();
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Failed to generate risk summary: {ex.Message}");
        }

        ConsoleHelper.WaitForKey();
    }

    private static string MapHealthEmoji(string? healthStatus)
    {
        return healthStatus switch
        {
            "AtRisk" => "🔴 AT RISK",
            "Attention" => "🟡 ATTENTION",
            "OnTrack" => "🟢 ON TRACK",
            _ => healthStatus ?? "Unknown"
        };
    }
}
