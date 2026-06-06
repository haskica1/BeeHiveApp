using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.OrgManagement;
using BeeHive.Application.Features.OrgManagement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

/// <summary>
/// Organization-scoped user assignment management. OrgAdmin can assign apiaries to ApiaryAdmins and
/// beehives to Beekeepers within their org; ApiaryAdmin can assign beehives from their own apiary.
/// All operations are scoped to the caller's organization in the service layer.
/// </summary>
[ApiController]
[Route("api/org")]
[Produces("application/json")]
[Authorize(Roles = Roles.OrgAdmin + "," + Roles.Admin)]
public class OrgManagementController : ControllerBase
{
    private readonly IOrgManagementService _service;

    public OrgManagementController(IOrgManagementService service)
    {
        _service = service;
    }

    /// <summary>Returns all User and Admin role members in the caller's organization.</summary>
    [HttpGet("members")]
    [ProducesResponseType(typeof(IEnumerable<OrgMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers()
    {
        var members = await _service.GetMembersAsync();
        return Ok(members);
    }

    /// <summary>Returns a single member with their current assignments.</summary>
    [HttpGet("members/{id:int}")]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMember(int id)
    {
        var member = await _service.GetMemberAsync(id);
        return Ok(member);
    }

    /// <summary>
    /// Updates beehive assignments for a User-role member.
    /// ApiaryAdmin callers may only assign beehives from their own apiary.
    /// </summary>
    [HttpPut("members/{id:int}/beehive-assignments")]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBeehiveAssignments(int id, [FromBody] UpdateBeehiveAssignmentsDto dto)
    {
        var updated = await _service.UpdateBeehiveAssignmentsAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>Updates the apiary assignment for an Admin-role member. OrgAdmin only.</summary>
    [HttpPut("members/{id:int}/apiary-assignment")]
    [Authorize(Roles = Roles.OrgAdmin)]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApiaryAssignment(int id, [FromBody] UpdateApiaryAssignmentDto dto)
    {
        var updated = await _service.UpdateApiaryAssignmentAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// Returns beehives available for assignment.
    /// OrgAdmin gets all org beehives; ApiaryAdmin gets only their apiary's beehives.
    /// </summary>
    [HttpGet("available-beehives")]
    [ProducesResponseType(typeof(IEnumerable<OrgAvailableBeehiveDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableBeehives()
    {
        var beehives = await _service.GetAvailableBeehivesAsync();
        return Ok(beehives);
    }

    /// <summary>Returns all apiaries in the organization for assigning to Admin users. OrgAdmin only.</summary>
    [HttpGet("available-apiaries")]
    [Authorize(Roles = Roles.OrgAdmin)]
    [ProducesResponseType(typeof(IEnumerable<OrgAvailableApiaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableApiaries()
    {
        var apiaries = await _service.GetAvailableApiariesAsync();
        return Ok(apiaries);
    }
}
