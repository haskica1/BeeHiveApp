using System.Security.Claims;
using BeeHive.Application.Features.Apiaries;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Application.Features.Weather;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

/// <summary>
/// Manages apiary (pčelinjak) resources.
/// All endpoints follow RESTful conventions and return Problem Details on error.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ApiariesController : ControllerBase
{
    private readonly IApiaryService _service;
    private readonly IWeatherService _weather;
    private readonly IValidator<CreateApiaryDto> _createValidator;
    private readonly IValidator<UpdateApiaryDto> _updateValidator;

    public ApiariesController(
        IApiaryService service,
        IWeatherService weather,
        IValidator<CreateApiaryDto> createValidator,
        IValidator<UpdateApiaryDto> updateValidator)
    {
        _service = service;
        _weather = weather;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>Returns all apiaries belonging to the current user's organization.
    /// Admin users only see their assigned apiary.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApiaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var orgIdClaim = User.FindFirstValue("organizationId");
        if (orgIdClaim == null) return Ok(Array.Empty<ApiaryDto>());
        var orgId = int.Parse(orgIdClaim);

        var apiaries = await _service.GetAllByOrganizationAsync(orgId);

        // Admin users are scoped to a single apiary (filtered by JWT claim).
        // If the claim is absent (misconfigured account) fall back to all org apiaries
        // so the user is never completely locked out.
        var role = User.FindFirstValue(ClaimTypes.Role);
        if (role == "Admin")
        {
            var apiaryIdClaim = User.FindFirstValue("apiaryId");
            if (apiaryIdClaim != null)
            {
                var apiaryId = int.Parse(apiaryIdClaim);
                return Ok(apiaries.Where(a => a.Id == apiaryId));
            }
        }

        return Ok(apiaries);
    }

    /// <summary>Returns a single apiary including its beehives.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiaryDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var apiary = await _service.GetByIdAsync(id);
        return Ok(apiary);
    }

    /// <summary>Creates a new apiary. OrgAdmin and SystemAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "OrgAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(ApiaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateApiaryDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var orgIdClaim = User.FindFirstValue("organizationId");
        if (orgIdClaim == null) return Forbid();
        var orgId = int.Parse(orgIdClaim);
        var userId = GetUserId();

        var created = await _service.CreateAsync(dto, orgId, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing apiary. OrgAdmin and SystemAdmin only.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "OrgAdmin,SystemAdmin")]
    [ProducesResponseType(typeof(ApiaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateApiaryDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.ToDictionary());

        var updated = await _service.UpdateAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Deletes an apiary and all its child beehives/inspections. OrgAdmin and SystemAdmin only.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "OrgAdmin,SystemAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    /// <summary>Returns a 7-day weather forecast for the apiary's location.</summary>
    [HttpGet("{id:int}/weather")]
    [ProducesResponseType(typeof(WeatherForecastDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWeather(int id)
    {
        var apiary = await _service.GetByIdAsync(id);

        if (!apiary.HasLocation)
            return BadRequest(new { message = "This apiary has no location set. Add latitude and longitude to enable weather forecasts." });

        var forecast = await _weather.GetForecastAsync(apiary.Latitude!.Value, apiary.Longitude!.Value);
        return Ok(forecast);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
