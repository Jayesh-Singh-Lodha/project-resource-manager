namespace PRM.Application.DTOs.Auth;

/// <summary>
/// Request payload for changing a user's password.
/// Both fields must match and meet password strength requirements.
/// </summary>
public record ChangePasswordRequest(string NewPassword, string ConfirmPassword);
