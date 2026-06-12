namespace PRM.Application.Interfaces;

public interface IEmailNotificationService
{
    /// <summary>
    /// Sends a reminder email to an employee for a missed timesheet.
    /// </summary>
    Task SendTimesheetReminderAsync(string toEmail, string employeeName, int reminderNumber);

    /// <summary>
    /// Sends an account freeze notification to both the employee and their manager.
    /// </summary>
    Task SendAccountFreezeNotificationAsync(string employeeEmail, string managerEmail, string employeeName);

    /// <summary>
    /// Sends a project at-risk notification to the project manager.
    /// </summary>
    Task SendProjectAtRiskNotificationAsync(string managerEmail, string managerName, string projectName, string healthStatus, string aiRiskSummary, string suggestedHelp, string milestonesSummary);
}
