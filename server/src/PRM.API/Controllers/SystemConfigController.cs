using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM.Application.DTOs.SystemConfig;
using PRM.Application.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/config")]
[Authorize(Roles = "Admin")]
public class SystemConfigController : ControllerBase
{
    private readonly ISystemConfigService _configService;

    public SystemConfigController(ISystemConfigService configService)
    {
        _configService = configService;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Get all system configurations", Description = "Returns all system configuration key-value pairs. Admin only.")]
    [ProducesResponseType(typeof(IEnumerable<SystemConfigResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllConfigs()
    {
        var configs = await _configService.GetAllConfigsAsync();
        return Ok(configs);
    }

    [HttpGet("{key}")]
    [SwaggerOperation(Summary = "Get configuration by key")]
    [ProducesResponseType(typeof(SystemConfigResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfigByKey(string key)
    {
        var config = await _configService.GetConfigByKeyAsync(key);
        if (config == null) return NotFound();
        return Ok(config);
    }

    [HttpPut("{key}")]
    [SwaggerOperation(Summary = "Update configuration", Description = "Updates the value of a specific configuration key.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateConfig(string key, [FromBody] UpdateSystemConfigRequest request)
    {
        await _configService.UpdateConfigAsync(key, request.Value);
        return NoContent();
    }
}
