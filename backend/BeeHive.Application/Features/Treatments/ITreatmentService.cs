using BeeHive.Application.Features.Treatments.DTOs;

namespace BeeHive.Application.Features.Treatments;

public interface ITreatmentService
{
    /// <summary>Role-scoped list, optionally filtered by apiary, hive, and/or year.</summary>
    Task<IEnumerable<TreatmentDto>> GetAllAsync(int? apiaryId, int? beehiveId, int? year);

    Task<TreatmentDetailDto> GetByIdAsync(int id);
    Task<TreatmentDetailDto> CreateAsync(CreateTreatmentDto dto);
    Task<TreatmentDetailDto> UpdateAsync(int id, UpdateTreatmentDto dto);
    Task DeleteAsync(int id);
}
