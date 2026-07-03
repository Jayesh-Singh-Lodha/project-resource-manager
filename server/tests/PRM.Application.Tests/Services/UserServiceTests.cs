using FluentAssertions;
using Moq;
using PRM.Application.DTOs.Users;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Enums;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;
using Xunit;

namespace PRM.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IAllocationRepository> _allocationRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _allocationRepositoryMock = new Mock<IAllocationRepository>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _allocationRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidData_ReturnsCreateUserResponse()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@test.com",
            FullName: "New User",
            Role: "Employee",
            Department: "Engineering"
        );

        _userRepositoryMock.Setup(repo => repo.ExistsAsync(request.Username, request.Email))
            .ReturnsAsync(false);

        var role = new Role { Id = 3, Name = "Employee" };
        _userRepositoryMock.Setup(repo => repo.GetRoleByNameAsync(request.Role))
            .ReturnsAsync(role);

        _passwordHasherMock.Setup(hasher => hasher.Hash(It.IsAny<string>()))
            .Returns("hashed_temp_password");

        _userRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newuser");
        result.Role.Should().Be("Employee");
        result.TemporaryPassword.Should().NotBeNullOrEmpty();
        _userRepositoryMock.Verify(repo => repo.AddAsync(It.Is<User>(u => u.Username == "newuser")), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateUser_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "existinguser",
            Email: "existing@test.com",
            FullName: "Existing User",
            Role: "Employee",
            Department: "Engineering"
        );

        _userRepositoryMock.Setup(repo => repo.ExistsAsync(request.Username, request.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.CreateUserAsync(request));
        exception.ErrorCode.Should().Be("DUPLICATE_USER");
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidRole_ThrowsDomainException()
    {
        // Arrange
        var request = new CreateUserRequest(
            Username: "newuser",
            Email: "newuser@test.com",
            FullName: "New User",
            Role: "InvalidRole",
            Department: "Engineering"
        );

        _userRepositoryMock.Setup(repo => repo.ExistsAsync(request.Username, request.Email))
            .ReturnsAsync(false);

        _userRepositoryMock.Setup(repo => repo.GetRoleByNameAsync(request.Role))
            .ReturnsAsync((Role)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.CreateUserAsync(request));
        exception.ErrorCode.Should().Be("INVALID_ROLE");
    }

    [Fact]
    public async Task DeactivateUserAsync_WithValidUser_SetsIsActiveToFalseAndEndsAllocations()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "employee",
            IsActive = true,
            Status = EmployeeStatus.Allocated
        };

        var activeAllocation = new Allocation
        {
            Id = 1,
            UserId = 1,
            FromDate = DateTime.UtcNow.AddDays(-10),
            ToDate = DateTime.UtcNow.AddDays(10),
            UtilisationPercent = 100
        };

        var allocations = new List<Allocation> { activeAllocation };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(user);

        _allocationRepositoryMock.Setup(repo => repo.GetByEmployeeIdAsync(1))
            .ReturnsAsync(allocations);

        // Act
        await _userService.DeactivateUserAsync(1);

        // Assert
        user.IsActive.Should().BeFalse();
        user.Status.Should().Be(EmployeeStatus.Bench);
        activeAllocation.ToDate.Date.Should().Be(DateTime.UtcNow.Date.AddDays(-1));
        _allocationRepositoryMock.Verify(repo => repo.UpdateAsync(activeAllocation), Times.Once);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }
    [Fact]
    public async Task GetAllUsersAsync_ReturnsUserList()
    {
        var users = new List<User>
        {
            new User { Id = 1, Username = "user1", Email = "u1@t.com", FullName = "U 1", Role = new Role { Name = "Employee" } },
            new User { Id = 2, Username = "user2", Email = "u2@t.com", FullName = "U 2", Role = new Role { Name = "Manager" } }
        };

        _userRepositoryMock.Setup(repo => repo.GetAllWithDetailsAsync()).ReturnsAsync(users);

        var result = await _userService.GetAllUsersAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUser()
    {
        var request = new UpdateUserRequest(
            FullName: "Updated Name",
            Department: "HR",
            Role: "Admin"
        );

        var user = new User { Id = 1, FullName = "Old Name" };
        var role = new Role { Id = 1, Name = "Admin" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetRoleByNameAsync("Admin")).ReturnsAsync(role);

        await _userService.UpdateUserAsync(1, request);

        user.FullName.Should().Be("Updated Name");
        user.RoleId.Should().Be(1);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }
    [Fact]
    public async Task ReactivateUserAsync_WithValidUser_SetsIsActiveToTrue()
    {
        var user = new User { Id = 1, IsActive = false };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);

        await _userService.ReactivateUserAsync(1);

        user.IsActive.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithValidPassword_UpdatesPassword()
    {
        var request = new ResetPasswordRequest("NewPassword123!");
        var user = new User { Id = 1, PasswordHash = "old" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.Hash("NewPassword123!")).Returns("new");

        await _userService.ResetPasswordAsync(1, request);

        user.PasswordHash.Should().Be("new");
        user.ForcePasswordChange.Should().BeTrue();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }
    [Fact]
    public async Task AddSkillAsync_WithValidData_AddsSkill()
    {
        var request = new AddSkillRequest(SkillName: "C#", Category: "Backend", ProficiencyLevel: "Advanced");
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _userRepositoryMock.Setup(repo => repo.GetSkillByNameAsync("C#")).ReturnsAsync(new Skill { Id = 1 });

        await _userService.AddSkillAsync(1, request);

        _userRepositoryMock.Verify(repo => repo.AddUserSkillAsync(It.Is<UserSkill>(s => s.SkillId == 1)), Times.Once);
    }

    [Fact]
    public async Task RemoveSkillAsync_WithValidData_RemovesSkill()
    {
        var userSkill = new UserSkill { UserId = 1, SkillId = 1 };
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(new User { Id = 1 });
        _userRepositoryMock.Setup(repo => repo.GetSkillByNameAsync("C#")).ReturnsAsync(new Skill { Id = 1 });
        _userRepositoryMock.Setup(repo => repo.GetUserSkillAsync(1, 1)).ReturnsAsync(userSkill);

        await _userService.RemoveSkillAsync(1, "C#");

        _userRepositoryMock.Verify(repo => repo.DeleteUserSkillAsync(userSkill), Times.Once);
    }

    [Fact]
    public async Task AssignManagerAsync_WithValidData_AssignsManager()
    {
        var user = new User { Id = 1 };
        var manager = new User { Id = 2, Role = new Role { Name = "Manager" } };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(manager);

        await _userService.AssignManagerAsync(1, 2);

        user.ManagerId.Should().Be(2);
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_WithInvalidId_ThrowsDomainException()
    {
        var request = new UpdateUserRequest("Name", "HR", "Admin");
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.UpdateUserAsync(1, request));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task DeactivateUserAsync_WithInvalidId_ThrowsDomainException()
    {
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.DeactivateUserAsync(1));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task ReactivateUserAsync_WithInvalidId_ThrowsDomainException()
    {
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.ReactivateUserAsync(1));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task AddSkillAsync_WithInvalidId_ThrowsDomainException()
    {
        var request = new AddSkillRequest(SkillName: "C#", Category: "Backend", ProficiencyLevel: "Advanced");
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.AddSkillAsync(1, request));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task RemoveSkillAsync_WithInvalidId_ThrowsDomainException()
    {
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _userService.RemoveSkillAsync(1, "C#"));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }
}
