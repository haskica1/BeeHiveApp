using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class DietRepository : Repository<Diet>, IDietRepository
{
    public DietRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Diet>> GetByBeehiveIdAsync(int beehiveId) =>
        await _context.Diets
            .AsNoTracking()
            .Include(d => d.FeedingEntries)
            .Include(d => d.CreatedBy)
            .Where(d => d.BeehiveId == beehiveId)
            .OrderByDescending(d => d.StartDate)
            .ToListAsync();

    public async Task<IEnumerable<Diet>> GetByBeehiveIdsAsync(IEnumerable<int> beehiveIds)
    {
        var ids = beehiveIds.ToList();
        if (ids.Count == 0) return Enumerable.Empty<Diet>();

        return await _context.Diets
            .AsNoTracking()
            .Include(d => d.FeedingEntries)
            .Where(d => ids.Contains(d.BeehiveId))
            .OrderByDescending(d => d.StartDate)
            .ToListAsync();
    }

    public async Task<Diet?> GetWithEntriesAsync(int id) =>
        await _context.Diets
            .Include(d => d.FeedingEntries)
            .Include(d => d.CreatedBy)
            .FirstOrDefaultAsync(d => d.Id == id);
}
