namespace PRM.Core.Entities;

/// <summary>
/// Links a user (resource) to a project for a date range with a utilisation percentage.
/// Business rule: sum of overlapping allocations for a user cannot exceed 100%.
/// </summary>
public class Allocation
{
    public int Id { get; set; }

    /// <summary>
    /// FK to the allocated user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// FK to the target project.
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Percentage of the user's capacity allocated to this project (1–100).
    /// </summary>
    public int UtilisationPercent { get; set; }

    /// <summary>
    /// Start date of the allocation period.
    /// </summary>
    public DateTime FromDate { get; set; }

    /// <summary>
    /// End date of the allocation period.
    /// </summary>
    public DateTime ToDate { get; set; }

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The user who is allocated.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The project the user is allocated to.
    /// </summary>
    public Project Project { get; set; } = null!;
}
