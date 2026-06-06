using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>Notification-specific data access operations.</summary>
public interface INotificationRepository : IRepository<Notification>
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAllAsReadAsync(int userId);
}
