using System.Security.Claims;
using BeeHive.Application.Features.OrgManagement;
using BeeHive.Application.Features.OrgManagement.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

/// <summary>
/// Organization-scoped user assignment management.
/// OrgAdmin can assign apiaries to Admins and beehives to Users within their org.
/// Admin can assign beehives (from their own apiary) to Users within their org.
/// </summary>
[ApiController]
[Route("api/org")]
[Produces("application/json")]
[Authorize(Roles = "OrgAdmin,Admin")]
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
        var orgId = GetOrgId();
        if (orgId == null) return Ok(Array.Empty<OrgMemberDto>());

        var members = await _service.GetMembersAsync(orgId.Value);
        return Ok(members);
    }

    /// <summary>Returns a single member with their current assignments.</summary>
    [HttpGet("members/{id:int}")]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMember(int id)
    {
        var orgId = GetOrgId();
        if (orgId == null) return Forbid();

        var member = await _service.GetMemberAsync(id, orgId.Value);
        return Ok(member);
    }

    /// <summary>
    /// Updates beehive assignments for a User-role member.
    /// Admin callers may only assign beehives from their own apiary.
    /// </summary>
    [HttpPut("members/{id:int}/beehive-assignments")]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBeehiveAssignments(int id, [FromBody] UpdateBeehiveAssignmentsDto dto)
    {
        var orgId = GetOrgId();
        if (orgId == null) return Forbid();

        var role = User.FindFirstValue(ClaimTypes.Role)!;
        var apiaryIdClaim = User.FindFirstValue("apiaryId");
        int? callerApiaryId = apiaryIdClaim != null ? int.Parse(apiaryIdClaim) : null;

        var updated = await _service.UpdateBeehiveAssignmentsAsync(id, dto, orgId.Value, callerApiaryId, role);
        return Ok(updated);
    }

    /// <summary>Updates the apiary assignment for an Admin-role member. OrgAdmin only.</summary>
    [HttpPut("members/{id:int}/apiary-assignment")]
    [Authorize(Roles = "OrgAdmin")]
    [ProducesResponseType(typeof(OrgMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApiaryAssignment(int id, [FromBody] UpdateApiaryAssignmentDto dto)
    {
        var orgId = GetOrgId();
        if (orgId == null) return Forbid();

        var updated = await _service.UpdateApiaryAssignmentAsync(id, dto, orgId.Value);
        return Ok(updated);
    }

    /// <summary>
    /// Returns beehives available for assignment.
    /// OrgAdmin gets all org beehives; Admin gets only their apiary's beehives.
    /// </summary>
    [HttpGet("available-beehives")]
    [ProducesResponseType(typeof(IEnumerable<OrgAvailableBeehiveDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableBeehives()
    {
        var orgId = GetOrgId();
        if (orgId == null) return Ok(Array.Empty<OrgAvailableBeehiveDto>());

        var apiaryIdClaim = User.FindFirstValue("apiaryId");
        int? callerApiaryId = apiaryIdClaim != null ? int.Parse(apiaryIdClaim) : null;

        var beehives = await _service.GetAvailableBeehivesAsync(orgId.Value, callerApiaryId);
        return Ok(beehives);
    }

    /// <summary>Returns all apiaries in the organization for assigning to Admin users. OrgAdmin only.</summary>
    [HttpGet("available-apiaries")]
    [Authorize(Roles = "OrgAdmin")]
    [ProducesResponseType(typeof(IEnumerable<OrgAvailableApiaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableApiaries()
    {
        var orgId = GetOrgId();
        if (orgId == null) return Ok(Array.Empty<OrgAvailableApiaryDto>());

        var apiaries = await _service.GetAvailableApiariesAsync(orgId.Value);
        return Ok(apiaries);
    }

    private int? GetOrgId()
    {
        var claim = User.FindFirstValue("organizationId");
        return claim != null ? int.Parse(claim) : null;
    }
}
