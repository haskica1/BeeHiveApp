using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using BeeHive.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data.Repositories;

// ── Organization Repository ───────────────────────────────────────────────────

public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
{
    public OrganizationRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<Organization>> GetAllWithDetailsAsync() =>
        await _context.Organizations
            .AsNoTracking()
            .Include(o => o.Users)
            .Include(o => o.Apiaries)
            .Include(o => o.CreatedBy)
            .OrderBy(o => o.Name)
            .ToListAsync();

    public async Task<Organization?> GetWithDetailsAsync(int id) =>
        await _context.Organizations
            .Include(o => o.Users)
            .Include(o => o.Apiaries)
            .Include(o => o.CreatedBy)
            .FirstOrDefaultAsync(o => o.Id == id);
}

// ── User Repository ───────────────────────────────────────────────────────────

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.Apiary)
            .FirstOrDefaultAsync(u => u.Email == email.ToLower());

    public async Task<IEnumerable<User>> GetAllWithOrganizationAsync() =>
        await _context.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .Include(u => u.Apiary)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

    public async Task<User?> GetByIdWithOrganizationAsync(int id) =>
        await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.Apiary)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByIdWithAssignedBeehivesAsync(int id) =>
        await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.Apiary)
            .Include(u => u.AssignedBeehives).ThenInclude(ub => ub.Beehive)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId) =>
        await _context.UserBeehives
            .AnyAsync(ub => ub.UserId == userId && ub.BeehiveId == beehiveId);

    public async Task SetBeehiveAssignmentsAsync(int userId, IEnumerable<int> beehiveIds)
    {
        var existing = await _context.UserBeehives
            .Where(ub => ub.UserId == userId)
            .ToListAsync();

        _context.UserBeehives.RemoveRange(existing);

        foreach (var beehiveId in beehiveIds)
            await _context.UserBeehives.AddAsync(new UserBeehive { UserId = userId, BeehiveId = beehiveId });
    }
}

// ── Apiary Repository ─────────────────────────────────────────────────────────

public class ApiaryRepository : Repository<Apiary>, IApiaryRepository
{
    public ApiaryRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<Apiary?> GetWithBeehivesAsync(int id) =>
        await _context.Apiaries
            .Include(a => a.Beehives).ThenInclude(b => b.Inspections)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Apiary>> GetAllWithBeehivesAsync() =>
        await _context.Apiaries
            .AsNoTracking()
            .Include(a => a.Beehives).ThenInclude(b => b.Inspections)
            .Include(a => a.CreatedBy)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public async Task<IEnumerable<Apiary>> GetAllByOrganizationAsync(int organizationId) =>
        await _context.Apiaries
            .AsNoTracking()
            .Include(a => a.Beehives).ThenInclude(b => b.Inspections)
            .Include(a => a.CreatedBy)
            .Where(a => a.OrganizationId == organizationId)
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
            .Include(b => b.CreatedBy)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<IEnumerable<Beehive>> GetByApiaryIdAsync(int apiaryId) =>
        await _context.Beehives
            .AsNoTracking()
            .Include(b => b.Inspections)
            .Include(b => b.CreatedBy)
            .Where(b => b.ApiaryId == apiaryId)
            .OrderBy(b => b.Name)
            .ToListAsync();

    public async Task<IEnumerable<Beehive>> GetByOrganizationAsync(int organizationId) =>
        await _context.Beehives
            .AsNoTracking()
            .Include(b => b.Apiary)
            .Where(b => b.Apiary.OrganizationId == organizationId)
            .OrderBy(b => b.Apiary.Name)
            .ThenBy(b => b.Name)
            .ToListAsync();

    public async Task<Beehive?> GetByUniqueIdAsync(Guid uniqueId) =>
        await _context.Beehives
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UniqueId == uniqueId);

    public async Task<IEnumerable<Beehive>> GetAllWithUniqueIdAsync() =>
        await _context.Beehives
            .Where(b => b.UniqueId != null)
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
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.ApiaryId == apiaryId)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Todo>> GetByBeehiveIdAsync(int beehiveId) =>
        await _context.Todos
            .AsNoTracking()
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .Where(t => t.BeehiveId == beehiveId)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

    public async Task<Todo?> GetByIdWithUsersAsync(int id) =>
        await _context.Todos
            .Include(t => t.CreatedBy)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);
}

// ── Diet Repository ───────────────────────────────────────────────────────────

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
