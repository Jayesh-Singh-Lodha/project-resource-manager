using PRM.Core.Entities;

namespace PRM.Core.Interfaces;

/// <summary>
/// Repository contract for User entity data access.
/// Implemented in PRM.Infrastructure.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Finds a user by their unique username (case-insensitive).
    /// Returns null if no user exists with that username.
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Finds a user by their primary key ID.
    /// Returns null if no user exists with that ID.
    /// </summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>
    /// Returns all users in the system.
    /// </summary>
    Task<IReadOnlyList<User>> GetAllAsync();
    Task<IReadOnlyList<User>> GetAllWithDetailsAsync();

    /// <summary>
    /// Returns all employees with their skills and allocations eager loaded for AI matching.
    /// </summary>
    Task<IReadOnlyList<User>> GetEmployeesWithDetailsAsync();

    /// <summary>
    /// Checks whether a user with the given username or email already exists.
    /// Used during user creation to prevent duplicates.
    /// </summary>
    Task<bool> ExistsAsync(string username, string email);

    /// <summary>
    /// Persists a new user to the database.
    /// </summary>
    Task AddAsync(User user);

    /// <summary>
    /// Updates an existing user record in the database.
    /// </summary>
    Task UpdateAsync(User user);

    /// <summary>
    /// Finds a Role by its name.
    /// </summary>
    Task<Role?> GetRoleByNameAsync(string roleName);

    Task<Skill?> GetSkillByNameAsync(string name);
    Task AddSkillEntityAsync(Skill skill);
    Task AddUserSkillAsync(UserSkill userSkill);
    Task UpdateUserSkillAsync(UserSkill userSkill);
    Task DeleteUserSkillAsync(UserSkill userSkill);
    Task<UserSkill?> GetUserSkillAsync(int userId, int skillId);
}
