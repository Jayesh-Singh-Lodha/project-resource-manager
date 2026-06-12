namespace PRM.Console.Models.Users;

/// <summary>
/// Request payload for creating a new user account.
/// Admin-only operation. Temp password is generated server-side.
/// </summary>
public record CreateUserRequest(
    string Username,
    string Email,
    string FullName,
    string Role,
    string? Department = null
);
