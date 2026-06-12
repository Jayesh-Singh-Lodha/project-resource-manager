using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Interfaces;

namespace PRM.Infrastructure.BackgroundJobs;

public class SchedulerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SchedulerService> _logger;
    private TimeSpan _interval = TimeSpan.FromMinutes(10); // Default fallback

    public SchedulerService(IServiceProvider serviceProvider, ILogger<SchedulerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Background Scheduler Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var configRepo = scope.ServiceProvider.GetRequiredService<ISystemConfigRepository>();
                
                // Read interval from DB
                var intervalConfig = await configRepo.GetByKeyAsync("SchedulerIntervalMinutes");
                if (intervalConfig != null && int.TryParse(intervalConfig.Value, out int minutes) && minutes > 0)
                {
                    _interval = TimeSpan.FromMinutes(minutes);
                }

                _logger.LogInformation("Running background jobs...");
                
                await UpdateEmployeeStatusesAsync(scope.ServiceProvider);
                await UpdateProjectHealthAsync(scope.ServiceProvider);
                await FlagMissedTimesheetsAsync(scope.ServiceProvider);

                _logger.LogInformation($"Background jobs completed. Sleeping for {_interval.TotalMinutes} minutes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while running background jobs.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task UpdateEmployeeStatusesAsync(IServiceProvider provider)
    {
        var userRepo = provider.GetRequiredService<IUserRepository>();
        var allocRepo = provider.GetRequiredService<IAllocationRepository>();

        var employeesList = await userRepo.GetAllAsync();
        var employees = employeesList.Where(u => u.Role.Name == "Employee" && u.IsActive).ToList();

        foreach (var emp in employees)
        {
            var allocations = await allocRepo.GetByEmployeeIdAsync(emp.Id);
            var activeAllocations = allocations.Where(a => a.FromDate <= DateTime.UtcNow && a.ToDate >= DateTime.UtcNow);
            
            var totalUtil = activeAllocations.Sum(a => a.UtilisationPercent);
            
            var newStatus = totalUtil >= 100 ? EmployeeStatus.Allocated : EmployeeStatus.Bench;
            
            if (emp.Status != newStatus)
            {
                emp.Status = newStatus;
                await userRepo.UpdateAsync(emp);
                _logger.LogInformation($"Updated status for Employee {emp.Id} to {newStatus} (Utilisation: {totalUtil}%)");
            }
        }
    }

    private async Task UpdateProjectHealthAsync(IServiceProvider provider)
    {
        var projectRepo = provider.GetRequiredService<IProjectRepository>();
        var userRepo = provider.GetRequiredService<IUserRepository>();
        var aiAssistant = provider.GetRequiredService<PRM.Application.Interfaces.IAiAssistantService>();
        var emailService = provider.GetRequiredService<PRM.Application.Interfaces.IEmailNotificationService>();
        
        var projects = await projectRepo.GetAllAsync();

        foreach (var proj in projects.Where(p => p.Status == ProjectStatus.Active))
        {
            var newHealth = HealthStatus.OnTrack;

            if (proj.EndDate < DateTime.UtcNow)
            {
                newHealth = HealthStatus.AtRisk;
            }
            else
            {
                var totalDays = (proj.EndDate - proj.StartDate).TotalDays;
                var daysPassed = (DateTime.UtcNow - proj.StartDate).TotalDays;
                
                if (totalDays > 0)
                {
                    var percentTimePassed = daysPassed / totalDays;

                    if (percentTimePassed > 0.9)
                    {
                        newHealth = HealthStatus.AtRisk;
                    }
                    else if (percentTimePassed > 0.7)
                    {
                        newHealth = HealthStatus.Attention;
                    }
                }
            }

            if (proj.HealthStatus != newHealth)
            {
                var oldHealth = proj.HealthStatus;
                proj.HealthStatus = newHealth;
                await projectRepo.UpdateAsync(proj);
                _logger.LogInformation($"Updated Health for Project {proj.Id} to {newHealth}");

                // Trigger Notification if changed to AtRisk
                if (newHealth == HealthStatus.AtRisk && proj.ManagerId.HasValue)
                {
                    try
                    {
                        var manager = await userRepo.GetByIdAsync(proj.ManagerId.Value);
                        if (manager != null)
                        {
                            // Fetch full project for milestones
                            var fullProject = await projectRepo.GetByIdAsync(proj.Id);
                            var milestonesSummary = fullProject?.Milestones != null && fullProject.Milestones.Any()
                                ? string.Join("\n", fullProject.Milestones.Select(m => $"- {m.Title} (Due: {m.DueDate:yyyy-MM-dd}, Status: {m.Status})"))
                                : "No milestones defined.";

                            var aiRiskSummary = await aiAssistant.GetProjectRiskSummaryAsync(proj.Id);
                            var suggestedHelp = await aiAssistant.GetSuggestedHelpForProjectAsync(proj.Id);

                            await emailService.SendProjectAtRiskNotificationAsync(
                                manager.Email,
                                manager.FullName,
                                proj.Name,
                                newHealth.ToString(),
                                aiRiskSummary,
                                suggestedHelp,
                                milestonesSummary
                            );
                            _logger.LogInformation($"Sent At-Risk notification for Project {proj.Id} to {manager.Email}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to process At-Risk notification for Project {proj.Id}");
                    }
                }
            }
        }
    }

    private async Task FlagMissedTimesheetsAsync(IServiceProvider provider)
    {
        var userRepo = provider.GetRequiredService<IUserRepository>();
        var timesheetRepo = provider.GetRequiredService<ITimesheetRepository>();
        var allocRepo = provider.GetRequiredService<IAllocationRepository>();
        var emailService = provider.GetRequiredService<PRM.Application.Interfaces.IEmailNotificationService>();

        var employeesList = await userRepo.GetAllAsync();
        var employees = employeesList.Where(u => u.Role.Name == "Employee" && u.IsActive).ToList();

        // Get last week's start date
        var diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
        var currentWeekStart = DateTime.UtcNow.AddDays(-1 * diff).Date;
        var lastWeekStart = currentWeekStart.AddDays(-7);
        var lastWeekEnd = lastWeekStart.AddDays(6);

        foreach (var emp in employees)
        {
            // Skip employees who had no active allocations during last week
            var allocations = await allocRepo.GetOverlappingAllocationsAsync(emp.Id, lastWeekStart, lastWeekEnd);
            if (!allocations.Any())
            {
                continue;
            }

            var timesheet = await timesheetRepo.GetByEmployeeAndWeekAsync(emp.Id, lastWeekStart);
            if (timesheet == null)
            {
                // Insert a MISSED timesheet record so it shows up in manager and employee views
                timesheet = new Timesheet
                {
                    UserId = emp.Id,
                    WeekStartDate = lastWeekStart,
                    Status = TimesheetStatus.Missed,
                    SubmittedAt = null,
                    ReminderCount = 0
                };
                await timesheetRepo.AddAsync(timesheet);
                _logger.LogWarning($"Employee {emp.Id} ({emp.FullName}) missed timesheet for week of {lastWeekStart:yyyy-MM-dd}. Created MISSED record.");
            }

            if (timesheet.Status == TimesheetStatus.Missed && !emp.IsTimesheetFrozen)
            {
                var now = DateTime.UtcNow;

                // Step 1: First Reminder
                if (timesheet.ReminderCount == 0)
                {
                    await emailService.SendTimesheetReminderAsync(emp.Email, emp.FullName, 1);
                    timesheet.ReminderCount = 1;
                    timesheet.LastReminderSentAt = now;
                    await timesheetRepo.UpdateAsync(timesheet);
                }
                // Step 2: Second Reminder (1 day after first reminder)
                else if (timesheet.ReminderCount == 1 && timesheet.LastReminderSentAt.HasValue && (now - timesheet.LastReminderSentAt.Value).TotalDays >= 1)
                {
                    await emailService.SendTimesheetReminderAsync(emp.Email, emp.FullName, 2);
                    timesheet.ReminderCount = 2;
                    timesheet.LastReminderSentAt = now;
                    await timesheetRepo.UpdateAsync(timesheet);
                }
                // Step 3: Freeze & Notify (1 day after second reminder)
                else if (timesheet.ReminderCount >= 2 && timesheet.LastReminderSentAt.HasValue && (now - timesheet.LastReminderSentAt.Value).TotalDays >= 1)
                {
                    emp.IsTimesheetFrozen = true;
                    await userRepo.UpdateAsync(emp);

                    var managerEmail = emp.Manager?.Email ?? "";
                    await emailService.SendAccountFreezeNotificationAsync(emp.Email, managerEmail, emp.FullName);

                    timesheet.ReminderCount = 3; // Mark as frozen handled
                    timesheet.LastReminderSentAt = now;
                    await timesheetRepo.UpdateAsync(timesheet);
                    
                    _logger.LogWarning($"Employee {emp.Id} timesheet access frozen due to missing timesheets.");
                }
            }
        }
    }
}
