using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data.Repositories;

// ── Apiary Repository ─────────────────────────────────────────────────────────

public class ApiaryRepository : Repository<Apiary>, IApiaryRepository
{
    public ApiaryRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<Apiary?> GetWithBeehivesAsync(int id) =>
        await _context.Apiaries
            .Include(a => a.Beehives)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Apiary>> GetAllWithBeehivesAsync() =>
        await _context.Apiaries
            .AsNoTracking()
            .Include(a => a.Beehives)
            .OrderBy(a => a.Name)
            .ToListAsync();
}

// ── Beehive Repository ────────────────────────────────────────────────────────

public class BeehiveRepository : Repository<Beehive>, IBeehiveRepository
{
    public BeehiveRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<Beehive?> GetWithInspectionsAsync(int id) =>
        await _context.Beehives
            .Include(b => b.Inspections.OrderByDescending(i => i.Date))
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Beehive>> GetByApiaryIdAsync(int apiaryId) =>
        await _context.Beehives
            .AsNoTracking()
            .Include(b => b.Inspections)
            .Where(b => b.ApiaryId == apiaryId)
            .OrderBy(b => b.Name)
            .ToListAsync();
}

// ── Inspection Repository ─────────────────────────────────────────────────────

public class InspectionRepository : Repository<Inspection>, IInspectionRepository
{
    public InspectionRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Inspection>> GetByBeehiveIdAsync(int beehiveId) =>
        await _context.Inspections
            .AsNoTracking()
            .Where(i => i.BeehiveId == beehiveId)
            .OrderByDescending(i => i.Date)
            .ToListAsync();
}

// ── Todo Repository ───────────────────────────────────────────────────────────

public class TodoRepository : Repository<Todo>, ITodoRepository
{
    public TodoRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Todo>> GetByApiaryIdAsync(int apiaryId) =>
        await _context.Todos
            .AsNoTracking()
            .Where(t => t.ApiaryId == apiaryId)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Todo>> GetByBeehiveIdAsync(int beehiveId) =>
        await _context.Todos
            .AsNoTracking()
            .Where(t => t.BeehiveId == beehiveId)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();
}

// ── Diet Repository ───────────────────────────────────────────────────────────

public class DietRepository : Repository<Diet>, IDietRepository
{
    public DietRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Diet>> GetByBeehiveIdAsync(int beehiveId) =>
        await _context.Diets
            .AsNoTracking()
            .Include(d => d.FeedingEntries)
            .Where(d => d.BeehiveId == beehiveId)
            .OrderByDescending(d => d.StartDate)
            .ToListAsync();

    public async Task<Diet?> GetWithEntriesAsync(int id) =>
        await _context.Diets
            .Include(d => d.FeedingEntries)
            .FirstOrDefaultAsync(d => d.Id == id);
}

// ── FeedingEntry Repository ───────────────────────────────────────────────────

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
