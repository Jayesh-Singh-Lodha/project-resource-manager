namespace PRM.Console.Models.Users;

/// <summary>
/// Response DTO for returning user information in list/detail views.
/// </summary>
public record UserResponse(
    int Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string? Department,
    string Status,
    bool IsActive,
    bool ForcePasswordChange,
    DateTime CreatedAt,
    int? ManagerId,
    List<string>? Skills = null,
    int CurrentUtilisationPercent = 0
);
