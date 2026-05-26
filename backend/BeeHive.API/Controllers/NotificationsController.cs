using System.Security.Claims;
using BeeHive.Application.Features.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BeeHive.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    /// <summary>Returns all notifications for the current user, plus unread count.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _service.GetForUserAsync(userId.Value);
        return Ok(result);
    }

    /// <summary>Marks all notifications for the current user as read.</summary>
    [HttpPatch("mark-all-read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _service.MarkAllAsReadAsync(userId.Value);
        return NoContent();
    }

    /// <summary>Marks a single notification as read.</summary>
    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        await _service.MarkAsReadAsync(id, userId.Value);
        return NoContent();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue("userId");
        return int.TryParse(claim, out var id) ? id : null;
    }
}
