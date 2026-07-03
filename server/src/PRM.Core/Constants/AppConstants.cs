namespace PRM.Core.Constants;

/// <summary>
/// Application-wide constants. No magic numbers — all values defined here.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Default admin username seeded in the database.
    /// </summary>
    public const string DefaultAdminUsername = "admin";

    /// <summary>
    /// Default admin email seeded in the database.
    /// </summary>
    public const string DefaultAdminEmail = "admin@prm.local";

    /// <summary>
    /// Default admin display name.
    /// </summary>
    public const string DefaultAdminFullName = "Administrator";

    /// <summary>
    /// Default admin password (must be changed on first login).
    /// </summary>
    public const string DefaultAdminPassword = "Admin@1234";

    /// <summary>
    /// Minimum password length enforced during creation and change.
    /// </summary>
    public const int PasswordMinLength = 8;

    /// <summary>
    /// Default maximum weekly working hours (configurable via SystemConfig).
    /// </summary>
    public const int DefaultMaxWeeklyHours = 40;

    /// <summary>
    /// Prefix for auto-generated temporary passwords when creating new users.
    /// Full format: TempPass@{4-digit random}.
    /// </summary>
    public const string TempPasswordPrefix = "TempPass@";

    // ── SystemConfig Keys ──────────────────────────

    /// <summary>
    /// SystemConfig key for maximum weekly working hours.
    /// </summary>
    public const string ConfigKeyMaxWeeklyHours = "MaxWeeklyHours";

    /// <summary>
    /// SystemConfig key for the active LLM provider name.
    /// </summary>
    public const string ConfigKeyLlmProvider = "LlmProvider";

    /// <summary>
    /// SystemConfig key for the background scheduler interval in minutes.
    /// </summary>
    public const string ConfigKeySchedulerIntervalMinutes = "SchedulerIntervalMinutes";
}
