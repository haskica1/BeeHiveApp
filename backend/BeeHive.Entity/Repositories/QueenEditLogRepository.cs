using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class QueenEditLogRepository : Repository<QueenEditLog>, IQueenEditLogRepository
{
    public QueenEditLogRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<QueenEditLog>> GetByQueenIdAsync(int queenId) =>
        await _context.QueenEditLogs
            .AsNoTracking()
            .Include(l => l.EditedBy)
            .Where(l => l.QueenId == queenId)
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .ToListAsync();
}
