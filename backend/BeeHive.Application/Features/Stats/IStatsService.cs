using BeeHive.Application.Features.Stats.DTOs;

namespace BeeHive.Application.Features.Stats;

public interface IStatsService
{
    /// <summary>Returns aggregate statistics scoped to the current caller's organization
    /// (platform-wide for SystemAdmin).</summary>
    Task<StatsDto> GetStatsAsync();
}
