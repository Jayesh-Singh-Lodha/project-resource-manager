namespace PRM.Core.Enums;

/// <summary>
/// Lifecycle status of a project.
/// Allocations are only allowed when status is Planned or Active.
/// </summary>
public enum ProjectStatus
{
    Planned,
    Active,
    OnHold,
    Completed
}
