namespace PRM.Core.Enums;

/// <summary>
/// Health indicator for a project.
/// Computed by the background scheduler based on milestone progress and timesheet data.
/// </summary>
public enum HealthStatus
{
    OnTrack,
    Attention,
    AtRisk
}
