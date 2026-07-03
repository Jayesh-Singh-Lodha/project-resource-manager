namespace PRM.Console.Models.Users;

/// <summary>
/// Response payload after successfully creating a user.
/// Includes the auto-generated temporary password (displayed once to the admin).
/// </summary>
public record CreateUserResponse(
    int Id,
    string Username,
    string Email,
    string FullName,
    string Role,
    string? Department,
    string TemporaryPassword,
    DateTime CreatedAt
);
