namespace PRM.Core.Enums;

/// <summary>
/// Status of a weekly timesheet.
/// Submitted = employee filed the timesheet. Missed = background scheduler flagged it.
/// </summary>
public enum TimesheetStatus
{
    Submitted,
    Missed
}
