using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

public interface IInspectionPhotoRepository : IRepository<InspectionPhoto>
{
    Task<IEnumerable<InspectionPhoto>> GetByInspectionIdAsync(int inspectionId);
    Task<int> CountByInspectionAsync(int inspectionId);
}
