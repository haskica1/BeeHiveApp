using System.Security.Claims;
using BeeHive.Application.Features.Calendar;
using BeeHive.Application.Features.Calendar.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _service;

    public CalendarController(ICalendarService service)
    {
        _service = service;
    }

    /// <summary>Returns all calendar events (todos with due dates + diet feeding entries) for the current user.</summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(CalendarEventsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents()
    {
        var role     = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId   = ParseClaim(ClaimTypes.NameIdentifier);
        var orgId    = ParseClaim("organizationId");
        var apiaryId = ParseClaim("apiaryId");

        var result = await _service.GetCalendarEventsAsync(role, userId, orgId, apiaryId);
        return Ok(result);
    }

    private int? ParseClaim(string name)
    {
        var value = User.FindFirstValue(name);
        return value != null && int.TryParse(value, out var id) ? id : null;
    }
}
