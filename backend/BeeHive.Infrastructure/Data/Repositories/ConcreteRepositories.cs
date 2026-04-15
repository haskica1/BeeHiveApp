using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
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
