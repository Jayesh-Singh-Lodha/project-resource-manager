using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// A named skill (e.g., "C#", "React", "Docker") with a category.
/// Skills are assigned to users via the UserSkill join entity.
/// </summary>
public class Skill
{
    public int Id { get; set; }

    /// <summary>
    /// Unique skill name (e.g., "C#", "Kubernetes").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Broad category the skill belongs to (Backend, Frontend, DevOps, QA, Other).
    /// </summary>
    public SkillCategory Category { get; set; }

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// Users who possess this skill.
    /// </summary>
    public List<UserSkill> UserSkills { get; set; } = [];
}
