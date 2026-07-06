using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Entity.Repositories;

public class InspectionPhotoRepository : Repository<InspectionPhoto>, IInspectionPhotoRepository
{
    public InspectionPhotoRepository(BeeHiveDbContext context) : base(context) { }

    public async Task<IEnumerable<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId) =>
        await _context.InspectionPhotos
            .AsNoTracking()
            .Where(p => p.InspectionId == inspectionId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync();

    public async Task<int> CountByInspectionAsync(int inspectionId) =>
        await _context.InspectionPhotos.CountAsync(p => p.InspectionId == inspectionId);
}
