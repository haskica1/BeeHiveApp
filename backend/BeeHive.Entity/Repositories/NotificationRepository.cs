using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<bool> ExistsRecentAsync(int userId, NotificationType type, int? relatedEntityId, DateTime since) =>
        await _context.Notifications.AnyAsync(n =>
            n.UserId == userId &&
            n.Type == type &&
            n.RelatedEntityId == relatedEntityId &&
            n.CreatedAt >= since);

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId) =>
        await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int userId) =>
        await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAllAsReadAsync(int userId)
    {
        await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
