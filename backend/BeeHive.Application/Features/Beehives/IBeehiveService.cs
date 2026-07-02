using BeeHive.Application.Features.Beehives.DTOs;

namespace BeeHive.Application.Features.Beehives;

public interface IBeehiveService
{
    Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId);
    Task<BeehiveDetailDto> GetByIdAsync(int id);
    Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto);
    Task<BeehiveDto> UpdateAsync(int id, UpdateBeehiveDto dto);
    Task DeleteAsync(int id);

    /// <summary>Public scan lookup — resolves a uniqueId to the minimal beehive info needed for redirect.</summary>
    Task<BeehiveScanDto?> GetScanInfoAsync(Guid uniqueId);

    /// <summary>QR codes of the apiary's beehives, for label printing (role-scoped like the list).</summary>
    Task<IEnumerable<BeehiveQrDto>> GetQrCodesByApiaryAsync(int apiaryId);

    /// <summary>Returns whether the current caller can view the beehive (used by the scan flow).</summary>
    Task<bool> CanCurrentUserAccessAsync(int beehiveId);

    /// <summary>Regenerates QR codes for all beehives using the current scan URL format. Returns count updated.</summary>
    Task<int> RegenerateAllQrCodesAsync();

    /// <summary>Returns all beehives accessible to the current user (role-scoped).</summary>
    Task<IEnumerable<BeehiveDto>> GetAllForCurrentUserAsync();
}
