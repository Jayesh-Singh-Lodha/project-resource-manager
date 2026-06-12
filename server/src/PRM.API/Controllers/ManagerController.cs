using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Allocations;
using PRM.Application.DTOs.Projects;
using PRM.Application.DTOs.Timesheets;
using PRM.Application.DTOs.Users;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/manager")]
[Authorize(Roles = "Manager,Admin")]
public class ManagerController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IProjectService _projectService;
    private readonly IAllocationService _allocationService;
    private readonly ITimesheetService _timesheetService;
    private readonly IAiAssistantService _aiAssistantService;

    public ManagerController(
        IUserService userService,
        IProjectService projectService,
        IAllocationService allocationService,
        ITimesheetService timesheetService,
        IAiAssistantService aiAssistantService)
    {
        _userService = userService;
        _projectService = projectService;
        _allocationService = allocationService;
        _timesheetService = timesheetService;
        _aiAssistantService = aiAssistantService;
    }

    [HttpGet("team")]
    [SwaggerOperation(Summary = "Get team members", Description = "Returns a list of all employees reporting to the specified manager.")]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeam()
    {
        var users = await _userService.GetAllUsersAsync();
        var managerId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
        var team = users.Where(u => u.Role == "Employee" && u.ManagerId == managerId);
        return Ok(team);
    }

    [HttpGet("projects")]
    [SwaggerOperation(Summary = "Get managed projects", Description = "Returns all projects where the specified user is the Project Manager.")]
    [ProducesResponseType(typeof(IEnumerable<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetManagedProjects()
    {
        var managerId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
        var projects = await _projectService.GetProjectsByManagerIdAsync(managerId);
        return Ok(projects);
    }

    [HttpGet("timesheets")]
    [SwaggerOperation(Summary = "Get team timesheets", Description = "Returns submitted timesheets for the manager's team for a given week.")]
    [ProducesResponseType(typeof(IEnumerable<TimesheetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamTimesheets([FromQuery] DateTime weekStartDate)
    {
        var managerId = int.Parse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)!);
        var timesheets = await _timesheetService.GetTeamTimesheetsAsync(managerId, weekStartDate);
        return Ok(timesheets);
    }

    [HttpPut("timesheets/{timesheetId}/status")]
    [SwaggerOperation(Summary = "Update timesheet status", Description = "Approves or rejects a timesheet.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateTimesheetStatus(int timesheetId, [FromBody] string status)
    {
        await _timesheetService.UpdateTimesheetStatusAsync(timesheetId, status);
        return NoContent();
    }

    [HttpPost("employees/{employeeId}/timesheets/restore")]
    [SwaggerOperation(Summary = "Restore timesheet access", Description = "Unfreezes timesheet access for an employee who missed multiple timesheets.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RestoreTimesheetAccess(int employeeId)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _timesheetService.RestoreTimesheetAccessAsync(employeeId, managerId);
        return NoContent();
    }

    [HttpPost("allocations")]
    [SwaggerOperation(Summary = "Create allocation", Description = "Allocates an employee to a project.")]
    [ProducesResponseType(typeof(AllocationResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAllocation([FromBody] CreateAllocationRequest request)
    {
        var response = await _allocationService.AllocateResourceAsync(request);
        return CreatedAtAction(nameof(GetManagedProjects), new { managerId = 0 }, response);
    }

    [HttpGet("allocations/search")]
    [SwaggerOperation(Summary = "Search resources via AI", Description = "Uses the configured LLM provider to search for resources matching criteria. Scoped to manager's team.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchResources([FromQuery] string criteria)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _aiAssistantService.SearchResourcesAsync(criteria, managerId);
        return Ok(new { Response = response });
    }

    [HttpGet("allocations/build-team")]
    [SwaggerOperation(Summary = "Build a team via AI", Description = "Uses the configured LLM provider to build a full team based on requirements. Scoped to manager's team.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> BuildTeam([FromQuery] string requirements)
    {
        var managerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _aiAssistantService.BuildTeamAsync(requirements, managerId);
        return Ok(new { Response = response });
    }

    /// <summary>
    /// Returns a single project by ID (must be managed by the current manager).
    /// </summary>
    [HttpGet("projects/{id}")]
    [SwaggerOperation(Summary = "Get project detail", Description = "Returns a single project by ID.")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }

    /// <summary>
    /// Returns all current allocations for a specific project.
    /// </summary>
    [HttpGet("projects/{projectId}/allocations")]
    [SwaggerOperation(Summary = "Get project allocations", Description = "Returns all allocations for a specific project.")]
    [ProducesResponseType(typeof(IEnumerable<AllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectAllocations(int projectId)
    {
        var allocations = await _allocationService.GetAllocationsByProjectIdAsync(projectId);
        return Ok(allocations);
    }

    /// <summary>
    /// Returns an AI-generated risk summary for a specific project.
    /// </summary>
    [HttpGet("projects/{id}/risk-summary")]
    [SwaggerOperation(Summary = "Get AI risk summary", Description = "Returns an AI-generated risk analysis for the specified project.")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectRiskSummary(int id)
    {
        var summary = await _aiAssistantService.GetProjectRiskSummaryAsync(id);
        return Ok(new { Summary = summary });
    }

    /// <summary>
    /// Returns detailed employee info including allocations and recent activity tags.
    /// Used by the Resource Dashboard drill-down.
    /// </summary>
    [HttpGet("employees/{id}/detail")]
    [SwaggerOperation(Summary = "Get employee detail", Description = "Returns employee allocations and recent timesheet activity for the dashboard drill-down.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeDetail(int id)
    {
        var users = await _userService.GetAllUsersAsync();
        var employee = users.FirstOrDefault(u => u.Id == id);
        if (employee is null)
            return NotFound();

        var allocations = await _allocationService.GetAllocationsByEmployeeIdAsync(id);
        var timesheets = await _timesheetService.GetTimesheetsByEmployeeIdAsync(id);

        // Extract recent activity tags from the last 4 weeks of timesheets
        var recentTags = timesheets
            .Where(t => t.WeekStartDate >= DateTime.UtcNow.AddDays(-28))
            .SelectMany(t => t.Entries)
            .Where(e => !string.IsNullOrWhiteSpace(e.ActivityTags))
            .SelectMany(e => e.ActivityTags!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .ToList();

        return Ok(new
        {
            Employee = employee,
            Allocations = allocations.Where(a => a.ToDate >= DateTime.UtcNow.Date).ToList(),
            RecentActivityTags = recentTags
        });
    }
}
