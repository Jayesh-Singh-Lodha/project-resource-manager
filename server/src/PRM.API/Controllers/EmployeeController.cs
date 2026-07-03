using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Allocations;
using PRM.Application.DTOs.Timesheets;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/employee")]
[Authorize(Roles = "Employee,Manager,Admin")]
public class EmployeeController : ControllerBase
{
    private readonly IAllocationService _allocationService;
    private readonly ITimesheetService _timesheetService;

    public EmployeeController(
        IAllocationService allocationService,
        ITimesheetService timesheetService)
    {
        _allocationService = allocationService;
        _timesheetService = timesheetService;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    [HttpGet("allocations")]
    [SwaggerOperation(Summary = "Get my allocations", Description = "Returns active and past project allocations for the authenticated employee.")]
    [ProducesResponseType(typeof(IEnumerable<AllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAllocations()
    {
        var employeeId = GetCurrentUserId();
        var allocations = await _allocationService.GetAllocationsByEmployeeIdAsync(employeeId);
        return Ok(allocations);
    }

    [HttpGet("timesheets")]
    [SwaggerOperation(Summary = "Get my timesheets", Description = "Returns all timesheets submitted by the authenticated employee.")]
    [ProducesResponseType(typeof(IEnumerable<TimesheetResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyTimesheets()
    {
        var employeeId = GetCurrentUserId();
        var timesheets = await _timesheetService.GetTimesheetsByEmployeeIdAsync(employeeId);
        return Ok(timesheets);
    }

    [HttpPost("timesheets")]
    [SwaggerOperation(Summary = "Submit timesheet", Description = "Submits a timesheet for a specific week.")]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitTimesheet([FromBody] SubmitTimesheetRequest request)
    {
        var employeeId = GetCurrentUserId();
        var secureRequest = request with { UserId = employeeId };

        var response = await _timesheetService.SubmitTimesheetAsync(secureRequest);
        return CreatedAtAction(nameof(GetMyTimesheets), new { }, response);
    }

    /// <summary>
    /// Returns the timesheet for last week. Used by the client to show the
    /// "missed timesheet" reminder banner on the Employee menu.
    /// Returns 204 No Content if no timesheet exists for last week (not submitted and not yet flagged).
    /// </summary>
    [HttpGet("timesheets/last-week-status")]
    [SwaggerOperation(Summary = "Get last week timesheet status", Description = "Returns the timesheet for last week, or 204 if none exists.")]
    [ProducesResponseType(typeof(TimesheetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetLastWeekTimesheetStatus()
    {
        var employeeId = GetCurrentUserId();
        var timesheet = await _timesheetService.GetLastWeekTimesheetAsync(employeeId);
        if (timesheet is null) return NoContent();
        return Ok(timesheet);
    }
}
