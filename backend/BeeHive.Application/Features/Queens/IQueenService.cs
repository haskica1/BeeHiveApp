using BeeHive.Application.Features.Queens.DTOs;

namespace BeeHive.Application.Features.Queens;

public interface IQueenService
{
    Task<IEnumerable<QueenDto>> GetByBeehiveIdAsync(int beehiveId);
    Task<QueenDto> CreateAsync(int beehiveId, CreateQueenDto dto);
    Task<QueenDto> UpdateAsync(int id, UpdateQueenDto dto);
    Task DeleteAsync(int id);
}
