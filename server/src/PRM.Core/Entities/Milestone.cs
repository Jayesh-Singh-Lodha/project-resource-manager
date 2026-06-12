using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// A deliverable milestone within a project.
/// Progress is tracked via StoryPoints and MilestoneStatus.
/// </summary>
public class Milestone
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the parent project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Short title describing the milestone deliverable.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Planned due date for milestone completion.
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Number of story points assigned to this milestone.
    /// </summary>
    public int StoryPoints { get; set; }

    /// <summary>
    /// Current progress status of the milestone.
    /// </summary>
    public MilestoneStatus Status { get; set; } = MilestoneStatus.NotStarted;

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The parent project this milestone belongs to.
    /// </summary>
    public Project Project { get; set; } = null!;
}
