using PRM.Application.DTOs.Users;

namespace PRM.Application.Interfaces;

/// <summary>
/// Contract for user management operations (Admin use cases).
/// Implemented in PRM.Application.Services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user account with a generated temporary password.
    /// Validates that username and email are unique.
    /// Sets ForcePasswordChange = true so the user must change password on first login.
    /// </summary>
    Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    /// Returns all users in the system.
    /// </summary>
    Task<IReadOnlyList<UserResponse>> GetAllUsersAsync();

    Task UpdateUserAsync(int userId, UpdateUserRequest request);
    Task DeactivateUserAsync(int userId);
    Task ReactivateUserAsync(int userId);
    Task ResetPasswordAsync(int userId, ResetPasswordRequest request);
    Task AddSkillAsync(int userId, AddSkillRequest request);
    Task UpdateSkillProficiencyAsync(int userId, UpdateSkillRequest request);
    Task RemoveSkillAsync(int userId, string skillName);
    Task AssignManagerAsync(int userId, int? managerId);
}

