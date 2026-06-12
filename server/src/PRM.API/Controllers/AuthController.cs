using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Auth;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

/// <summary>
/// Handles authentication endpoints: login and password change.
/// Thin controller — all business logic delegated to IAuthService.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates a user with username and password.
    /// Returns a JWT token and user metadata on success.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <returns>JWT token, role, force password change flag, and full name.</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Login to the system", Description = "Authenticates a user with username and password. Returns a JWT token and user metadata on success.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Changes the authenticated user's password.
    /// Requires a valid JWT Bearer token.
    /// Sets ForcePasswordChange to false after a successful change.
    /// </summary>
    /// <param name="request">New password and confirmation.</param>
    [HttpPost("change-password")]
    [Authorize]
    [SwaggerOperation(Summary = "Change password", Description = "Changes the authenticated user's password. Sets ForcePasswordChange to false after a successful change.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");

        if (userIdClaim is null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        await _authService.ChangePasswordAsync(userId, request);
        return NoContent();
    }
}
