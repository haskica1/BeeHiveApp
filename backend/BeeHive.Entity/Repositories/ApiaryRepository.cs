using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

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
