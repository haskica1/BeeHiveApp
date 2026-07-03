using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class PastureRepository : Repository<Pasture>, IPastureRepository
{
    public PastureRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<(Pasture Pasture, int ApiariesOnPasture)>> GetByOrganizationWithCountsAsync(int organizationId)
    {
        var rows = await _context.Pastures
            .AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .OrderBy(p => p.Name)
            .Select(p => new
            {
                Pasture = p,
                ApiariesOnPasture = _context.Apiaries.Count(a => a.CurrentPastureId == p.Id),
            })
            .ToListAsync();

        return rows.Select(r => (r.Pasture, r.ApiariesOnPasture));
    }

    public async Task<bool> HasReferencesAsync(int pastureId) =>
        await _context.Apiaries.AnyAsync(a => a.CurrentPastureId == pastureId) ||
        await _context.ApiaryMoves.AnyAsync(m => m.ToPastureId == pastureId || m.FromPastureId == pastureId);
}
