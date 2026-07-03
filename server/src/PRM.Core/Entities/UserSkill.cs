using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// Join entity linking a User to a Skill with a proficiency level.
/// Composite PK: (UserId, SkillId).
/// </summary>
public class UserSkill
{
    public int UserId { get; set; }

    public int SkillId { get; set; }

    /// <summary>
    /// How proficient the user is in this skill (Beginner, Intermediate, Advanced).
    /// </summary>
    public ProficiencyLevel ProficiencyLevel { get; set; }

    // ── Navigation Properties ──────────────────────────

    public User User { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
