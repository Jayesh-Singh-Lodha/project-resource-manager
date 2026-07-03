namespace PRM.Application.DTOs.Auth;

/// <summary>
/// Response payload returned on successful login.
/// Contains the JWT token, user role, and force-password-change flag.
/// </summary>
public record LoginResponse(
    string Token,
    string Role,
    bool ForcePasswordChange,
    string FullName
);
