namespace PRM.Core.Entities;

/// <summary>
/// A single line item within a timesheet, recording hours for one project.
/// Business rule: hours_worked cannot exceed allocation% × max_weekly_hours.
/// </summary>
public class TimesheetEntry
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the parent timesheet.
    /// </summary>
    public int TimesheetId { get; set; }

    /// <summary>
    /// FK to the project these hours are logged against.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Number of hours worked on this project during the week.
    /// </summary>
    public decimal HoursWorked { get; set; }

    /// <summary>
    /// Comma-separated activity tags (e.g., "Backend API, Bug Fixing").
    /// </summary>
    public string? ActivityTags { get; set; }

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The parent timesheet this entry belongs to.
    /// </summary>
    public Timesheet Timesheet { get; set; } = null!;

    /// <summary>
    /// The project hours are logged against.
    /// </summary>
    public Project Project { get; set; } = null!;
}
