using BeeHive.Application.Features.Notifications.DTOs;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Notifications;

public interface INotificationService
{
    Task NotifyAsync(
        int userId,
        string title,
        string message,
        NotificationType type,
        int? relatedEntityId = null,
        string? relatedEntityType = null);

    Task<NotificationListDto> GetForUserAsync(int userId);
    Task MarkAllAsReadAsync(int userId);
    Task MarkAsReadAsync(int notificationId, int userId);
}
