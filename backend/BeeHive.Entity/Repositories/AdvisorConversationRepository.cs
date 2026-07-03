using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class AdvisorConversationRepository : Repository<AdvisorConversation>, IAdvisorConversationRepository
{
    public AdvisorConversationRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<AdvisorConversation>> GetByUserAsync(int userId) =>
        await _context.AdvisorConversations
            .AsNoTracking()
            .Include(c => c.Beehive)
            .Where(c => c.UserId == userId)
            // UpdatedAt is bumped on every exchange, so it doubles as "last activity".
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .ToListAsync();

    public async Task<AdvisorConversation?> GetWithMessagesAsync(int id) =>
        await _context.AdvisorConversations
            .Include(c => c.Beehive)
            .Include(c => c.Messages.OrderBy(m => m.Id))
            .FirstOrDefaultAsync(c => c.Id == id);
}
