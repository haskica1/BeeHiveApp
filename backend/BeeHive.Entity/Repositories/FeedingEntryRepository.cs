using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class FeedingEntryRepository : Repository<FeedingEntry>, IFeedingEntryRepository
{
    public FeedingEntryRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<FeedingEntry>> GetByDietIdAsync(int dietId) =>
        await _context.FeedingEntries
            .AsNoTracking()
            .Where(e => e.DietId == dietId)
            .OrderBy(e => e.ScheduledDate)
            .ToListAsync();
}
