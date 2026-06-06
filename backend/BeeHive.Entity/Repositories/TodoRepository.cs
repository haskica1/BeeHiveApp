using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

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
