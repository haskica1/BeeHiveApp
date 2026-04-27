using System.Security.Claims;
using BeeHive.Application.Features.Admin;
using BeeHive.Application.Features.Admin.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

/// <summary>
/// System administration endpoints — accessible only to SystemAdmin users.
/// Provides full CRUD for organizations and users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize(Roles = "SystemAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;

    public AdminController(IAdminService service)
    {
        _service = service;
    }

    // ── Organizations ──────────────────────────────────────────────────────────

    [HttpGet("organizations")]
    [ProducesResponseType(typeof(IEnumerable<AdminOrganizationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrganizations()
    {
        var orgs = await _service.GetAllOrganizationsAsync();
        return Ok(orgs);
    }

    [HttpGet("organizations/{id:int}")]
    [ProducesResponseType(typeof(AdminOrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrganization(int id)
    {
        var org = await _service.GetOrganizationByIdAsync(id);
        return Ok(org);
    }

    [HttpPost("organizations")]
    [ProducesResponseType(typeof(AdminOrganizationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDto dto)
    {
        var userId = GetUserId();
        var created = await _service.CreateOrganizationAsync(dto, userId);
        return CreatedAtAction(nameof(GetOrganization), new { id = created.Id }, created);
    }

    [HttpPut("organizations/{id:int}")]
    [ProducesResponseType(typeof(AdminOrganizationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto dto)
    {
        var updated = await _service.UpdateOrganizationAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("organizations/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        await _service.DeleteOrganizationAsync(id);
        return NoContent();
    }

    /// <summary>Returns apiaries for a given organization — used when assigning Admin users.</summary>
    [HttpGet("organizations/{id:int}/apiaries")]
    [ProducesResponseType(typeof(IEnumerable<AdminApiaryListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiariesByOrganization(int id)
    {
        var apiaries = await _service.GetApiariesByOrganizationAsync(id);
        return Ok(apiaries);
    }

    /// <summary>Returns all beehives for a given organization — used when assigning User role beekeepers.</summary>
    [HttpGet("organizations/{id:int}/beehives")]
    [ProducesResponseType(typeof(IEnumerable<AdminBeehiveListItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBeehivesByOrganization(int id)
    {
        var beehives = await _service.GetBeehivesByOrganizationAsync(id);
        return Ok(beehives);
    }

    // ── Users ──────────────────────────────────────────────────────────────────

    [HttpGet("users")]
    [ProducesResponseType(typeof(IEnumerable<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _service.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{id:int}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _service.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPost("users")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateAdminUserDto dto)
    {
        var created = await _service.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }

    [HttpPut("users/{id:int}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateAdminUserDto dto)
    {
        var updated = await _service.UpdateUserAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("users/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _service.DeleteUserAsync(id);
        return NoContent();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim) : null;
    }
}
