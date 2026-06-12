using PRM.Application.DTOs.Auth;

namespace PRM.Application.Interfaces;

/// <summary>
/// Contract for authentication operations.
/// Implemented in PRM.Application.Services.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// Returns a JWT token and user metadata on success.
    /// Throws InvalidCredentialsException on failure.
    /// Throws AccountInactiveException if the account is deactivated.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Changes the password for the specified user.
    /// Validates password strength and confirmation match.
    /// Sets ForcePasswordChange to false after a successful change.
    /// </summary>
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
