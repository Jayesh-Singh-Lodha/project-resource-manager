using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Users;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

/// <summary>
/// Handles admin-only endpoints: user management.
/// All actions require the Admin role via JWT claims.
/// Thin controller — all business logic delegated to IUserService.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Creates a new user account with a generated temporary password.
    /// The temporary password is included in the response (shown once to the admin).
    /// The new user must change their password on first login.
    /// </summary>
    /// <param name="request">User creation details: username, email, full name, role, department.</param>
    /// <returns>Created user details including the one-time temporary password.</returns>
    [HttpPost("users")]
    [SwaggerOperation(Summary = "Create a new user account", Description = "Creates a new user account with a generated temporary password. The new user must change their password on first login.")]
    [ProducesResponseType(typeof(CreateUserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var response = await _userService.CreateUserAsync(request);
        return CreatedAtAction(nameof(GetAllUsers), null, response);
    }

    /// <summary>
    /// Returns all users in the system.
    /// </summary>
    [HttpGet("users")]
    [SwaggerOperation(Summary = "Get all users", Description = "Returns a list of all users in the system.")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}")]
    [SwaggerOperation(Summary = "Update user details", Description = "Updates basic user details (name, department, role).")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        await _userService.UpdateUserAsync(id, request);
        return NoContent();
    }

    [HttpPost("users/{id}/deactivate")]
    [SwaggerOperation(Summary = "Deactivate user", Description = "Deactivates a user, ending active allocations and preventing login.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        await _userService.DeactivateUserAsync(id);
        return NoContent();
    }

    [HttpPost("users/{id}/reactivate")]
    [SwaggerOperation(Summary = "Reactivate user", Description = "Reactivates a user account, allowing login again.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReactivateUser(int id)
    {
        await _userService.ReactivateUserAsync(id);
        return NoContent();
    }

    [HttpPost("users/{id}/reset-password")]
    [SwaggerOperation(Summary = "Reset user password", Description = "Resets a user's password and sets the force password change flag.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request)
    {
        await _userService.ResetPasswordAsync(id, request);
        return NoContent();
    }

    [HttpPost("users/{id}/skills")]
    [SwaggerOperation(Summary = "Add skill to user", Description = "Assigns a new skill to the user with a proficiency level.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddSkill(int id, [FromBody] AddSkillRequest request)
    {
        await _userService.AddSkillAsync(id, request);
        return NoContent();
    }

    [HttpPut("users/{id}/skills")]
    [SwaggerOperation(Summary = "Update skill proficiency", Description = "Updates the proficiency level of an existing skill.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateSkillProficiency(int id, [FromBody] UpdateSkillRequest request)
    {
        await _userService.UpdateSkillProficiencyAsync(id, request);
        return NoContent();
    }

    [HttpDelete("users/{id}/skills/{skillName}")]
    [SwaggerOperation(Summary = "Remove skill", Description = "Removes a skill from a user.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveSkill(int id, string skillName)
    {
        await _userService.RemoveSkillAsync(id, skillName);
        return NoContent();
    }

    [HttpPost("users/{id}/manager")]
    [SwaggerOperation(Summary = "Assign manager", Description = "Assigns a manager to an employee.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AssignManager(int id, [FromQuery] int? managerId)
    {
        await _userService.AssignManagerAsync(id, managerId);
        return NoContent();
    }
}
