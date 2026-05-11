using System.Security.Claims;
using BeeHive.Application.Features.Stats;
using BeeHive.Application.Features.Stats.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IStatsService _service;

    public StatsController(IStatsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(StatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats()
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        int? organizationId = null;
        if (role != "SystemAdmin")
        {
            var orgClaim = User.FindFirstValue("organizationId");
            if (int.TryParse(orgClaim, out var orgId))
                organizationId = orgId;
        }

        var result = await _service.GetStatsAsync(organizationId);
        return Ok(result);
    }
}
