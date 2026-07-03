using PRM.Application.DTOs.Users;
using PRM.Application.Interfaces;
using PRM.Application.Validators;
using PRM.Core.Constants;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

/// <summary>
/// Implements user management business logic (Admin use cases).
/// Handles user creation with temporary password generation and validation.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAllocationRepository _allocationRepository;

    public UserService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IAllocationRepository allocationRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _allocationRepository = allocationRepository;
    }


    /// <inheritdoc />
    public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request)
    {
        ValidateCreateUserRequest(request);

        if (await _userRepository.ExistsAsync(request.Username, request.Email))
        {
            throw new DomainException(
                "A user with this username or email already exists.",
                "DUPLICATE_USER");
        }

        var role = await _userRepository.GetRoleByNameAsync(request.Role);
        if (role is null)
        {
            throw new DomainException(
                $"Invalid role '{request.Role}'. Valid roles: Admin, Manager, Employee.",
                "INVALID_ROLE");
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var user = new User
        {
            Username = request.Username.Trim().ToLower(),
            Email = request.Email.Trim().ToLower(),
            FullName = request.FullName.Trim(),
            PasswordHash = _passwordHasher.Hash(temporaryPassword),
            RoleId = role.Id,
            Role = role,
            Department = request.Department?.Trim(),
            IsActive = true,
            ForcePasswordChange = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return new CreateUserResponse(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.Name,
            Department: user.Department,
            TemporaryPassword: temporaryPassword,
            CreatedAt: user.CreatedAt
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllWithDetailsAsync();

        return users.Select(user => new UserResponse(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.Name,
            Department: user.Department,
            Status: user.Status.ToString(),
            IsActive: user.IsActive,
            ForcePasswordChange: user.ForcePasswordChange,
            IsTimesheetFrozen: user.IsTimesheetFrozen,
            CreatedAt: user.CreatedAt,
            ManagerId: user.ManagerId,
            Skills: user.UserSkills?.Select(us => us.Skill.Name).ToList() ?? new List<string>(),
            CurrentUtilisationPercent: user.Allocations?
                .Where(a => a.ToDate >= DateTime.UtcNow.Date)
                .Sum(a => a.UtilisationPercent) ?? 0
        )).ToList().AsReadOnly();
    }

    private static void ValidateCreateUserRequest(CreateUserRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Username))
            errors.Add("Username is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");

        if (string.IsNullOrWhiteSpace(request.FullName))
            errors.Add("Full name is required.");

        if (string.IsNullOrWhiteSpace(request.Role))
            errors.Add("Role is required.");

        if (errors.Count > 0)
        {
            throw new DomainException(
                "Validation failed for create user request.",
                "VALIDATION_ERROR")
            {
                Data = { ["Errors"] = errors }
            };
        }
    }

    private static string GenerateTemporaryPassword()
    {
        var random = new Random();
        var digits = random.Next(1000, 9999);
        return $"{AppConstants.TempPasswordPrefix}{digits}";
    }

    public async Task UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        var roleEntity = await _userRepository.GetRoleByNameAsync(request.Role);
        if (roleEntity is null) throw new DomainException("Role not found in DB.", "ROLE_NOT_FOUND");

        user.FullName = request.FullName.Trim();
        user.Department = request.Department?.Trim();
        user.RoleId = roleEntity.Id;
        user.Role = roleEntity;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task DeactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        // End allocations today
        var activeAllocations = await _allocationRepository.GetByEmployeeIdAsync(userId);
        foreach (var alloc in activeAllocations.Where(a => a.ToDate >= DateTime.UtcNow.Date))
        {
            alloc.ToDate = DateTime.UtcNow.Date.AddDays(-1);
            await _allocationRepository.UpdateAsync(alloc);
        }

        user.Status = EmployeeStatus.Bench;

        await _userRepository.UpdateAsync(user);
    }

    public async Task ReactivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task ResetPasswordAsync(int userId, ResetPasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        var validationErrors = PasswordValidator.Validate(request.NewTemporaryPassword);
        if (validationErrors.Count > 0)
        {
            throw new DomainException(
                "Password does not meet strength requirements.",
                "WEAK_PASSWORD")
            {
                Data = { ["Errors"] = validationErrors }
            };
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewTemporaryPassword);
        user.ForcePasswordChange = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }

    public async Task AddSkillAsync(int userId, AddSkillRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        if (!Enum.TryParse<SkillCategory>(request.Category, ignoreCase: true, out var category))
        {
            throw new DomainException("Invalid skill category.", "INVALID_CATEGORY");
        }

        if (!Enum.TryParse<ProficiencyLevel>(request.ProficiencyLevel, ignoreCase: true, out var level))
        {
            throw new DomainException("Invalid proficiency level.", "INVALID_PROFICIENCY");
        }

        var skill = await _userRepository.GetSkillByNameAsync(request.SkillName);
        if (skill is null)
        {
            skill = new Skill { Name = request.SkillName.Trim(), Category = category };
            await _userRepository.AddSkillEntityAsync(skill);
        }

        var userSkill = await _userRepository.GetUserSkillAsync(userId, skill.Id);
        if (userSkill is not null)
        {
            throw new DomainException("Employee already has this skill.", "DUPLICATE_SKILL");
        }

        userSkill = new UserSkill
        {
            UserId = userId,
            SkillId = skill.Id,
            ProficiencyLevel = level
        };

        await _userRepository.AddUserSkillAsync(userSkill);
    }

    public async Task UpdateSkillProficiencyAsync(int userId, UpdateSkillRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        var skill = await _userRepository.GetSkillByNameAsync(request.SkillName);
        if (skill is null) throw new DomainException("Skill not found.", "SKILL_NOT_FOUND");

        var userSkill = await _userRepository.GetUserSkillAsync(userId, skill.Id);
        if (userSkill is null)
        {
            throw new DomainException("Employee does not have this skill assigned.", "SKILL_NOT_ASSIGNED");
        }

        if (!Enum.TryParse<ProficiencyLevel>(request.ProficiencyLevel, ignoreCase: true, out var level))
        {
            throw new DomainException("Invalid proficiency level.", "INVALID_PROFICIENCY");
        }

        userSkill.ProficiencyLevel = level;
        await _userRepository.UpdateUserSkillAsync(userSkill);
    }

    public async Task RemoveSkillAsync(int userId, string skillName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        var skill = await _userRepository.GetSkillByNameAsync(skillName);
        if (skill is null) throw new DomainException("Skill not found.", "SKILL_NOT_FOUND");

        var userSkill = await _userRepository.GetUserSkillAsync(userId, skill.Id);
        if (userSkill is null)
        {
            throw new DomainException("Employee does not have this skill assigned.", "SKILL_NOT_ASSIGNED");
        }

        await _userRepository.DeleteUserSkillAsync(userSkill);
    }

    public async Task AssignManagerAsync(int userId, int? managerId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) throw new DomainException("User not found.", "USER_NOT_FOUND");

        if (managerId.HasValue)
        {
            var manager = await _userRepository.GetByIdAsync(managerId.Value);
            if (manager is null) throw new DomainException("Manager not found.", "MANAGER_NOT_FOUND");
            if (manager.Role.Name != "Manager") throw new DomainException("Assigned user is not a Manager.", "INVALID_MANAGER");
        }

        user.ManagerId = managerId;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}

