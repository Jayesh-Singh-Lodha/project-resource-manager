using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// Represents a managed project with milestones, allocations, and health tracking.
/// </summary>
public class Project
{
    public int Id { get; set; }

    /// <summary>
    /// Display name of the project.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional long-form description of the project scope.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Project start date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Planned project end date.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Current lifecycle status of the project.
    /// </summary>
    public ProjectStatus Status { get; set; } = ProjectStatus.Planned;

    /// <summary>
    /// FK to the User who manages this project.
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// Total story points across all milestones.
    /// </summary>
    public int TotalStoryPoints { get; set; }

    /// <summary>
    /// Health indicator computed by the background scheduler.
    /// </summary>
    public HealthStatus HealthStatus { get; set; } = HealthStatus.OnTrack;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The project manager (User entity).
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Milestones that make up this project's deliverables.
    /// </summary>
    public List<Milestone> Milestones { get; set; } = [];

    /// <summary>
    /// Resource allocations for this project.
    /// </summary>
    public List<Allocation> Allocations { get; set; } = [];
}
