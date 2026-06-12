using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.Projects;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Create a project", Description = "Creates a new project. Admin only.")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var response = await _projectService.CreateProjectAsync(request);
        return CreatedAtAction(nameof(GetProjectById), new { id = response.Id }, response);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all projects", Description = "Returns all projects.")]
    [ProducesResponseType(typeof(IEnumerable<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Get project by ID")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectById(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update a project", Description = "Updates project details. Admin only.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        await _projectService.UpdateProjectAsync(id, request);
        return NoContent();
    }

    [HttpPost("{id}/milestones")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Add a milestone", Description = "Adds a milestone to a project. Admin only.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AddMilestone(int id, [FromBody] AddMilestoneRequest request)
    {
        await _projectService.AddMilestoneAsync(id, request);
        return NoContent();
    }

    [HttpPut("milestones/{milestoneId}/status")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Update milestone status", Description = "Updates the status of a milestone. Admin only.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMilestoneStatus(int milestoneId, [FromBody] UpdateMilestoneStatusRequest request)
    {
        await _projectService.UpdateMilestoneStatusAsync(milestoneId, request);
        return NoContent();
    }

    [HttpGet("{id}/milestones")]
    [SwaggerOperation(Summary = "Get project milestones")]
    [ProducesResponseType(typeof(IEnumerable<MilestoneResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMilestonesByProjectId(int id)
    {
        var milestones = await _projectService.GetMilestonesByProjectIdAsync(id);
        return Ok(milestones);
    }
}
