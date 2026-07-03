using BeeHive.Application.Features.Pastures.DTOs;

namespace BeeHive.Application.Features.Pastures;

public interface IApiaryMoveService
{
    /// <summary>Move history, newest first — anyone who can view the apiary.</summary>
    Task<IEnumerable<ApiaryMoveDto>> GetByApiaryAsync(int apiaryId);

    /// <summary>
    /// Records a move: resolves FromPasture server-side, sets the apiary's current pasture, and
    /// snapshots the pasture coordinates into the apiary (weather/map follow automatically).
    /// </summary>
    Task<ApiaryMoveDto> CreateAsync(int apiaryId, CreateApiaryMoveDto dto);

    /// <summary>Mistake correction: only the latest move can be deleted; reverts pasture + coordinates.</summary>
    Task DeleteAsync(int apiaryId, int moveId);
}
