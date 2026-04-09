using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Inspections;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IInspectionService
{
    Task<IEnumerable<InspectionDto>> GetByBeehiveIdAsync(int beehiveId);
    Task<InspectionDto> GetByIdAsync(int id);
    Task<InspectionDto> CreateAsync(CreateInspectionDto dto);
    Task<InspectionDto> UpdateAsync(int id, UpdateInspectionDto dto);
    Task DeleteAsync(int id);
}

// ── Implementation ───────────────────────────────────────────────────────────

public class InspectionService : IInspectionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public InspectionService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InspectionDto>> GetByBeehiveIdAsync(int beehiveId)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        var inspections = await _uow.Inspections.GetByBeehiveIdAsync(beehiveId);
        return _mapper.Map<IEnumerable<InspectionDto>>(inspections);
    }

    public async Task<InspectionDto> GetByIdAsync(int id)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task<InspectionDto> CreateAsync(CreateInspectionDto dto)
    {
        if (!await _uow.Beehives.ExistsAsync(dto.BeehiveId))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId);

        var inspection = _mapper.Map<Inspection>(dto);
        await _uow.Inspections.AddAsync(inspection);
        await _uow.SaveChangesAsync();

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task<InspectionDto> UpdateAsync(int id, UpdateInspectionDto dto)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        if (!await _uow.Beehives.ExistsAsync(dto.BeehiveId))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId);

        _mapper.Map(dto, inspection);
        inspection.UpdatedAt = DateTime.UtcNow;

        await _uow.Inspections.UpdateAsync(inspection);
        await _uow.SaveChangesAsync();

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task DeleteAsync(int id)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        await _uow.Inspections.DeleteAsync(inspection);
        await _uow.SaveChangesAsync();
    }
}
