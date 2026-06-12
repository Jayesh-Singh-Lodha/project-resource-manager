using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// Weekly timesheet header for a user.
/// Contains one or more TimesheetEntry records (one per project).
/// Business rule: only one timesheet per user per week.
/// </summary>
public class Timesheet
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the user who submitted the timesheet.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The Monday of the week this timesheet covers.
    /// </summary>
    public DateTime WeekStartDate { get; set; }

    /// <summary>
    /// Whether the timesheet was submitted by the user or flagged as missed.
    /// </summary>
    public TimesheetStatus Status { get; set; }

    /// <summary>
    /// Timestamp when the user submitted the timesheet. Null if status is Missed.
    /// </summary>
    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// Tracks how many timesheet reminders have been sent to the user.
    /// </summary>
    public int ReminderCount { get; set; } = 0;

    /// <summary>
    /// Timestamp of when the last reminder was sent to the user.
    /// </summary>
    public DateTime? LastReminderSentAt { get; set; }

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The user who owns this timesheet.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Individual project-level entries within this timesheet.
    /// </summary>
    public List<TimesheetEntry> Entries { get; set; } = [];
}
