using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Beehives;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IBeehiveService
{
    Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId);
    Task<BeehiveDetailDto> GetByIdAsync(int id);
    Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto);
    Task<BeehiveDto> UpdateAsync(int id, UpdateBeehiveDto dto);
    Task DeleteAsync(int id);
}

// ── Implementation ───────────────────────────────────────────────────────────

public class BeehiveService : IBeehiveService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BeehiveService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId)
    {
        // Validate parent apiary exists
        if (!await _uow.Apiaries.ExistsAsync(apiaryId))
            throw new NotFoundException(nameof(Apiary), apiaryId);

        var beehives = await _uow.Beehives.GetByApiaryIdAsync(apiaryId);
        return _mapper.Map<IEnumerable<BeehiveDto>>(beehives);
    }

    public async Task<BeehiveDetailDto> GetByIdAsync(int id)
    {
        var beehive = await _uow.Beehives.GetWithInspectionsAsync(id)
            ?? throw new NotFoundException(nameof(Beehive), id);

        return _mapper.Map<BeehiveDetailDto>(beehive);
    }

    public async Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto)
    {
        // Ensure the target apiary exists before creating the beehive
        if (!await _uow.Apiaries.ExistsAsync(dto.ApiaryId))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        var beehive = _mapper.Map<Beehive>(dto);
        await _uow.Beehives.AddAsync(beehive);
        await _uow.SaveChangesAsync();

        return _mapper.Map<BeehiveDto>(beehive);
    }

    public async Task<BeehiveDto> UpdateAsync(int id, UpdateBeehiveDto dto)
    {
        var beehive = await _uow.Beehives.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Beehive), id);

        if (!await _uow.Apiaries.ExistsAsync(dto.ApiaryId))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        _mapper.Map(dto, beehive);
        beehive.UpdatedAt = DateTime.UtcNow;

        await _uow.Beehives.UpdateAsync(beehive);
        await _uow.SaveChangesAsync();

        return _mapper.Map<BeehiveDto>(beehive);
    }

    public async Task DeleteAsync(int id)
    {
        var beehive = await _uow.Beehives.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Beehive), id);

        await _uow.Beehives.DeleteAsync(beehive);
        await _uow.SaveChangesAsync();
    }
}
