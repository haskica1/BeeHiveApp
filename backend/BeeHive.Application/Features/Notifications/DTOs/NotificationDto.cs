namespace BeeHive.Application.Features.Notifications.DTOs;

public record NotificationDto(
    int Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt,
    int? RelatedEntityId,
    string? RelatedEntityType
);

public record NotificationListDto(
    IEnumerable<NotificationDto> Notifications,
    int UnreadCount
);
