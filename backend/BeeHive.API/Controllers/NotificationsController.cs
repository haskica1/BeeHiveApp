using System.Security.Claims;
using BeeHive.Application.Common.Interfaces;
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
    private readonly IEmailService _email;

    public NotificationsController(INotificationService service, IEmailService email)
    {
        _service = service;
        _email   = email;
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

    /// <summary>Sends a test email to the current user's address. SystemAdmin only.</summary>
    [HttpPost("test-email")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> TestEmail()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var emailClaim = User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)
                      ?? User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(emailClaim))
            return BadRequest("Could not determine email from token.");

        try
        {
            await _email.SendAsync(
                emailClaim,
                "Test User",
                "BeeHive — SMTP Test",
                "<h2>✅ SMTP is working!</h2><p>If you received this, email delivery is configured correctly.</p>");

            return Ok(new { message = $"Test email sent to {emailClaim}." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
        }
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
