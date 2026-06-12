using FluentAssertions;
using Moq;
using PRM.Application.DTOs.Auth;
using PRM.Application.Services;
using PRM.Core.Entities;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;
using Xunit;

namespace PRM.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginRequest("testuser", "Password123!");
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashed_password",
            IsActive = true,
            ForcePasswordChange = false,
            Role = new Role { Name = "Admin" },
            FullName = "Test User"
        };

        _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(hasher => hasher.Verify(request.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenServiceMock.Setup(service => service.GenerateToken(user))
            .Returns("valid_token");

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("valid_token");
        result.Role.Should().Be("Admin");
        result.FullName.Should().Be("Test User");
        result.ForcePasswordChange.Should().BeFalse();
    }

    [Fact]
    public async Task LoginAsync_WithInvalidUsername_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("wronguser", "Password123!");
        _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest("testuser", "WrongPassword!");
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashed_password",
            IsActive = true
        };

        _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        _passwordHasherMock.Setup(hasher => hasher.Verify(request.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveAccount_ThrowsAccountInactiveException()
    {
        // Arrange
        var request = new LoginRequest("testuser", "Password123!");
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            PasswordHash = "hashed_password",
            IsActive = false
        };

        _userRepositoryMock.Setup(repo => repo.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<AccountInactiveException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task ChangePasswordAsync_WithValidData_ChangesPassword()
    {
        var request = new ChangePasswordRequest("NewPass123!", "NewPass123!");
        var user = new User { Id = 1, PasswordHash = "hashed_old" };

        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
        _passwordHasherMock.Setup(hasher => hasher.Hash("NewPass123!")).Returns("hashed_new");

        await _authService.ChangePasswordAsync(1, request);

        user.PasswordHash.Should().Be("hashed_new");
        user.ForcePasswordChange.Should().BeFalse();
        _userRepositoryMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WithInvalidUser_ThrowsDomainException()
    {
        var request = new ChangePasswordRequest("NewPass123!", "NewPass123!");
        _userRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((User)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _authService.ChangePasswordAsync(1, request));
        exception.ErrorCode.Should().Be("USER_NOT_FOUND");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithMismatchedPasswords_ThrowsDomainException()
    {
        // Arrange
        var request = new ChangePasswordRequest("NewPassword123!", "DifferentPassword123!");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _authService.ChangePasswordAsync(1, request));
        exception.ErrorCode.Should().Be("PASSWORD_MISMATCH");
    }

    [Fact]
    public async Task ChangePasswordAsync_WithWeakPassword_ThrowsDomainException()
    {
        // Arrange
        var request = new ChangePasswordRequest("weak", "weak");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _authService.ChangePasswordAsync(1, request));
        exception.ErrorCode.Should().Be("WEAK_PASSWORD");
    }
}
