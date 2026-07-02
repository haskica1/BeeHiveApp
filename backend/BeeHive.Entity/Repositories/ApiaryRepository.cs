using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class ApiaryRepository : Repository<Apiary>, IApiaryRepository
{
    public ApiaryRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<Apiary?> GetWithBeehivesAsync(int id) =>
        await _context.Apiaries
            .Include(a => a.Beehives)
            .Include(a => a.CreatedBy)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Apiary>> GetAllByOrganizationAsync(int organizationId) =>
        await _context.Apiaries
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId)
            .OrderBy(a => a.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<(Apiary Apiary, int BeehiveCount)>> GetByOrganizationWithCountsAsync(int organizationId)
    {
        var rows = await _context.Apiaries
            .AsNoTracking()
            .Where(a => a.OrganizationId == organizationId)
            .OrderBy(a => a.Name)
            .Select(a => new { Apiary = a, a.CreatedBy, BeehiveCount = a.Beehives.Count })
            .ToListAsync();

        return rows.Select(r =>
        {
            r.Apiary.CreatedBy = r.CreatedBy; // reattach the separately projected navigation
            return (r.Apiary, r.BeehiveCount);
        }).ToList();
    }
}
