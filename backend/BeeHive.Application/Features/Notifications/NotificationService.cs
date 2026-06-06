using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Notifications.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BeeHive.Application.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _uow;
    private readonly IEmailService _email;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IUnitOfWork uow, IEmailService email, ILogger<NotificationService> logger)
    {
        _uow    = uow;
        _email  = email;
        _logger = logger;
    }

    public async Task NotifyAsync(
        int userId,
        string title,
        string message,
        NotificationType type,
        int? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var notification = new Notification
        {
            UserId            = userId,
            Title             = title,
            Message           = message,
            Type              = type,
            RelatedEntityId   = relatedEntityId,
            RelatedEntityType = relatedEntityType,
        };

        await _uow.Notifications.AddAsync(notification);
        await _uow.SaveChangesAsync();

        // Fetch the user by email directly to avoid EF change-tracker issues
        var users = await _uow.Users.FindAsync(u => u.Id == userId);
        var user  = users.FirstOrDefault();

        if (user == null)
        {
            _logger.LogWarning("NotifyAsync: user {UserId} not found — skipping email", userId);
            return;
        }

        _logger.LogInformation("NotifyAsync: sending email to {Email} for [{Type}] {Title}", user.Email, type, title);

        await _email.SendAsync(user.Email, $"{user.FirstName} {user.LastName}",
            $"BeeHive — {title}", BuildEmailHtml($"{user.FirstName} {user.LastName}", title, message));
    }

    public async Task<NotificationListDto> GetForUserAsync(int userId)
    {
        var notifications = await _uow.Notifications.GetByUserIdAsync(userId);
        var unreadCount   = await _uow.Notifications.GetUnreadCountAsync(userId);
        var dtos = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Title,
            n.Message,
            n.Type.ToString(),
            n.IsRead,
            n.CreatedAt,
            n.RelatedEntityId,
            n.RelatedEntityType));

        return new NotificationListDto(dtos, unreadCount);
    }

    public async Task MarkAllAsReadAsync(int userId) =>
        await _uow.Notifications.MarkAllAsReadAsync(userId);

    public async Task MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _uow.Notifications.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != userId) return;

        notification.IsRead = true;
        await _uow.Notifications.UpdateAsync(notification);
        await _uow.SaveChangesAsync();
    }

    private static string BuildEmailHtml(string name, string title, string message) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:sans-serif;background:#fef9ee;padding:32px">
          <div style="max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:32px;border:1px solid #f6dfa0">
            <h2 style="color:#92400e;margin-top:0">🐝 BeeHive Notification</h2>
            <p style="color:#374151">Hi <strong>{name}</strong>,</p>
            <div style="background:#fef3c7;border-radius:8px;padding:16px;margin:16px 0">
              <strong style="color:#92400e">{title}</strong>
              <p style="color:#374151;margin:8px 0 0">{message}</p>
            </div>
            <p style="color:#6b7280;font-size:12px;margin-bottom:0">
              You are receiving this because you have an account on BeeHive App.
            </p>
          </div>
        </body>
        </html>
        """;
}
