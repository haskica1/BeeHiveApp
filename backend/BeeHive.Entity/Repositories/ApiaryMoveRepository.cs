using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class ApiaryMoveRepository : Repository<ApiaryMove>, IApiaryMoveRepository
{
    public ApiaryMoveRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<ApiaryMove>> GetByApiaryAsync(int apiaryId) =>
        await _context.ApiaryMoves
            .AsNoTracking()
            .Include(m => m.FromPasture)
            .Include(m => m.ToPasture)
            .Include(m => m.CreatedBy)
            .Where(m => m.ApiaryId == apiaryId)
            .OrderByDescending(m => m.MovedAt)
            .ThenByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

    public async Task<ApiaryMove?> GetLatestForApiaryAsync(int apiaryId) =>
        await _context.ApiaryMoves
            .AsNoTracking()
            .Include(m => m.FromPasture)
            .Include(m => m.ToPasture)
            .Where(m => m.ApiaryId == apiaryId)
            .OrderByDescending(m => m.MovedAt)
            .ThenByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<ApiaryMove>> GetByApiariesAsync(IReadOnlyCollection<int> apiaryIds) =>
        await _context.ApiaryMoves
            .AsNoTracking()
            .Include(m => m.ToPasture)
            .Where(m => apiaryIds.Contains(m.ApiaryId))
            .ToListAsync();
}
