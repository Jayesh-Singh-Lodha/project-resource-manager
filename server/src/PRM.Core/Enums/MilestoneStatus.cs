namespace PRM.Core.Enums;

/// <summary>
/// Progress status of a project milestone.
/// Used by the background scheduler to flag project health.
/// </summary>
public enum MilestoneStatus
{
    NotStarted,
    InProgress,
    Done
}
