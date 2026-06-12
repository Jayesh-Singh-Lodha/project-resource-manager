namespace PRM.Core.Entities;

/// <summary>
/// Key-value configuration store for runtime settings.
/// Examples: LLM provider, API key, scheduler interval, max weekly hours.
/// PK is the config key (string).
/// </summary>
public class SystemConfig
{
    /// <summary>
    /// Configuration key (e.g., "LlmProvider", "MaxWeeklyHours").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Configuration value stored as a string. Consumers parse as needed.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
