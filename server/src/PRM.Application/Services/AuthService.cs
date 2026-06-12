using PRM.Application.DTOs.Auth;
using PRM.Application.Interfaces;
using PRM.Application.Validators;
using PRM.Core.Exceptions;
using PRM.Core.Interfaces;

namespace PRM.Application.Services;

/// <summary>
/// Implements authentication business logic.
/// Validates credentials, generates JWT tokens, and handles password changes.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidCredentialsException();
        }

        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new AccountInactiveException();
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResponse(
            Token: token,
            Role: user.Role.Name,
            ForcePasswordChange: user.ForcePasswordChange,
            FullName: user.FullName
        );
    }

    /// <inheritdoc />
    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new DomainException(
                "New password and confirmation do not match.",
                "PASSWORD_MISMATCH");
        }

        var validationErrors = PasswordValidator.Validate(request.NewPassword);
        if (validationErrors.Count > 0)
        {
            throw new DomainException(
                "Password does not meet strength requirements.",
                "WEAK_PASSWORD")
            {
                Data = { ["Errors"] = validationErrors }
            };
        }

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null)
        {
            throw new DomainException("User not found.", "USER_NOT_FOUND");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.ForcePasswordChange = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
    }
}
