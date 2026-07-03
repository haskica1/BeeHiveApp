using BeeHive.Application.Features.Harvests.DTOs;

namespace BeeHive.Application.Features.Harvests;

public interface IHarvestService
{
    /// <summary>Role-scoped list of harvests, optionally filtered by apiary and/or year.</summary>
    Task<IEnumerable<HarvestDto>> GetAllAsync(int? apiaryId, int? year);

    Task<HarvestDetailDto> GetByIdAsync(int id);
    Task<HarvestDetailDto> CreateAsync(CreateHarvestDto dto);
    Task<HarvestDetailDto> UpdateAsync(int id, UpdateHarvestDto dto);
    Task DeleteAsync(int id);

    /// <summary>Season + per-year honey yield for a single hive (access = viewing the hive).</summary>
    Task<HiveYieldDto> GetHiveYieldAsync(int beehiveId);
}
