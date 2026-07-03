using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Allocations;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/allocations")]
[Authorize]
public class AllocationController : ControllerBase
{
    private readonly IAllocationService _allocationService;

    public AllocationController(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Manager")]
    [SwaggerOperation(Summary = "Get all allocations", Description = "Returns all resource allocations across all projects.")]
    [ProducesResponseType(typeof(IEnumerable<AllocationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAllocations()
    {
        var allocations = await _allocationService.GetAllAllocationsAsync();
        return Ok(allocations);
    }

    /// <summary>
    /// Ends an allocation by setting its end date to today.
    /// Updates the employee's status accordingly.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    [SwaggerOperation(Summary = "End an allocation", Description = "Ends an active allocation by setting its end date to today. Updates employee status.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> EndAllocation(int id)
    {
        await _allocationService.EndAllocationAsync(id);
        return NoContent();
    }
}
