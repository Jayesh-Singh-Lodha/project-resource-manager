using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PRM.Application.DTOs.Config;
using PRM.Application.Interfaces;

namespace PRM.Infrastructure.ExternalServices;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly SmtpSettings _smtpSettings;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailNotificationService> logger)
    {
        _smtpSettings = smtpSettings.Value;
        _logger = logger;
    }

    public async Task SendTimesheetReminderAsync(string toEmail, string employeeName, int reminderNumber)
    {
        var subject = $"Action Required: Timesheet Reminder #{reminderNumber}";
        var body = $"Hello {employeeName},\n\nThis is Reminder #{reminderNumber} that your timesheet from last week has not been submitted. Please submit it as soon as possible to avoid account restrictions.\n\nThank you,\nPRM System";

        await SendEmailAsync(toEmail, subject, body);
    }

    public async Task SendAccountFreezeNotificationAsync(string employeeEmail, string managerEmail, string employeeName)
    {
        var subject = "Account Restricted: Missing Timesheets";
        var body = $"Hello,\n\n{employeeName}'s timesheet submission access has been frozen due to multiple unsubmitted timesheets. The employee can still log in but cannot submit new entries.\n\nManager action is required to restore access.\n\nThank you,\nPRM System";

        // Send to employee
        await SendEmailAsync(employeeEmail, subject, body);
        
        // Send to manager if email is available
        if (!string.IsNullOrWhiteSpace(managerEmail))
        {
            await SendEmailAsync(managerEmail, subject, body);
        }
    }

    public async Task SendProjectAtRiskNotificationAsync(string managerEmail, string managerName, string projectName, string healthStatus, string aiRiskSummary, string suggestedHelp, string milestonesSummary)
    {
        var subject = $"Project At-Risk Notification: {projectName}";
        
        var body = $@"Hello {managerName},

The project '{projectName}' has been flagged as At-Risk by the Project Health Scheduler.

Project Details:
Name: {projectName}
Manager: {managerName}

Milestones at a glance:
{milestonesSummary}

Health Status:
Current standing: {healthStatus}

AI Risk Summary:
{aiRiskSummary}

Suggested Help:
{suggestedHelp}

Please review the project details and take necessary actions.

Thank you,
PRM System";

        await SendEmailAsync(managerEmail, subject, body);
    }

    private async Task SendEmailAsync(string to, string subject, string body)
    {
        // Fallback to logger if SMTP settings are missing
        if (string.IsNullOrWhiteSpace(_smtpSettings.Server) || string.IsNullOrWhiteSpace(_smtpSettings.SenderEmail))
        {
            _logger.LogInformation("SMTP not configured. Mock Email Sent:\nTo: {To}\nSubject: {Subject}\nBody: {Body}", to, subject, body);
            return;
        }

        try
        {
            using var client = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email successfully sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}. Falling back to log record.", to);
            _logger.LogInformation("Fallback Email Sent:\nTo: {To}\nSubject: {Subject}\nBody: {Body}", to, subject, body);
        }
    }
}
