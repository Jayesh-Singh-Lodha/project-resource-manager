using PRM.Core.Enums;

namespace PRM.Core.Entities;

/// <summary>
/// Represents a user account in the PRM system.
/// Users are created by Admins and authenticate via username/password.
/// Each user is assigned a single role that determines their menu access.
/// In the merged User-Employee model, this entity also carries employee
/// fields like Department, ManagerId, and EmployeeStatus.
/// </summary>
public class User
{
    public int Id { get; set; }

    /// <summary>
    /// Unique login identifier. Cannot be changed after creation.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Unique email address for the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown in menus and reports.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt-hashed password. Never stored in plain text.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// The role ID assigned to this user (FK to Role).
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// The role navigation property.
    /// Determines which console menu is displayed after login.
    /// </summary>
    public Role Role { get; set; } = null!;


    /// <summary>
    /// Organisational department the user belongs to. Nullable for Admin users.
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Self-referencing FK to the user's manager.
    /// Null for top-level admins or users without a manager assigned.
    /// </summary>
    public int? ManagerId { get; set; }

    /// <summary>
    /// Current allocation status — Bench (no active allocations) or Allocated.
    /// Computed when allocations change.
    /// </summary>
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Bench;

    /// <summary>
    /// When false, the user cannot log in. Set by Admin deactivation.
    /// Historical data (timesheets, allocations) is preserved.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When true, the user must change their password before accessing any menu.
    /// Set to true when account is created by Admin or password is reset.
    /// </summary>
    public bool ForcePasswordChange { get; set; } = true;

    /// <summary>
    /// When true, the user's timesheet submission access is frozen due to missed timesheets.
    /// The user can still log in and view, but cannot submit new entries until access is restored.
    /// </summary>
    public bool IsTimesheetFrozen { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ── Navigation Properties ──────────────────────────

    /// <summary>
    /// The user's direct manager. Null if no manager assigned.
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Users who report directly to this user (as manager).
    /// </summary>
    public List<User> DirectReports { get; set; } = [];

    /// <summary>
    /// Skills possessed by this user with proficiency levels.
    /// </summary>
    public List<UserSkill> UserSkills { get; set; } = [];

    /// <summary>
    /// Resource allocations for this user across projects.
    /// </summary>
    public List<Allocation> Allocations { get; set; } = [];

    /// <summary>
    /// Timesheets submitted by this user.
    /// </summary>
    public List<Timesheet> Timesheets { get; set; } = [];

    /// <summary>
    /// Projects managed by this user (where this user is the project manager).
    /// </summary>
    public List<Project> ManagedProjects { get; set; } = [];
}
