using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Core.Interfaces;
using PRM.Infrastructure.Data;

namespace PRM.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of the User repository.
/// All data access for User entities goes through this class.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly PrmDbContext _context;

    public UserRepository(PrmDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }

    /// <inheritdoc />
    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }
    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetAllWithDetailsAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
            .Include(u => u.Allocations).ThenInclude(a => a.Project)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }
    /// <inheritdoc />
    public async Task<IReadOnlyList<User>> GetEmployeesWithDetailsAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
            .Include(u => u.Allocations).ThenInclude(a => a.Project)
            .Where(u => u.Role.Name == "Employee" && u.IsActive)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }


    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string username, string email)
    {
        return await _context.Users
            .AnyAsync(u =>
                u.Username.ToLower() == username.ToLower() ||
                u.Email.ToLower() == email.ToLower());
    }

    /// <inheritdoc />
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Role?> GetRoleByNameAsync(string roleName)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower());
    }

    public async Task<Skill?> GetSkillByNameAsync(string name)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task AddSkillEntityAsync(Skill skill)
    {
        await _context.Skills.AddAsync(skill);
        await _context.SaveChangesAsync();
    }

    public async Task AddUserSkillAsync(UserSkill userSkill)
    {
        await _context.UserSkills.AddAsync(userSkill);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserSkillAsync(UserSkill userSkill)
    {
        _context.UserSkills.Update(userSkill);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserSkillAsync(UserSkill userSkill)
    {
        _context.UserSkills.Remove(userSkill);
        await _context.SaveChangesAsync();
    }

    public async Task<UserSkill?> GetUserSkillAsync(int userId, int skillId)
    {
        return await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);
    }
}

