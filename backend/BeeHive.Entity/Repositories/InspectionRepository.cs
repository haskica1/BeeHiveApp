using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

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
