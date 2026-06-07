using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Application.Features.Weather;
using BeeHive.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace BeeHive.Application.Features.Inspections;

public class InspectionService : IInspectionService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IAccessGuard _access;
    private readonly IWeatherService _weather;
    private readonly ILogger<InspectionService> _logger;

    public InspectionService(
        IUnitOfWork uow,
        IMapper mapper,
        IAccessGuard access,
        IWeatherService weather,
        ILogger<InspectionService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _access = access;
        _weather = weather;
        _logger = logger;
    }

    public async Task<IEnumerable<InspectionDto>> GetByBeehiveIdAsync(int beehiveId)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        await _access.EnsureCanAccessBeehiveAsync(beehiveId);

        var inspections = await _uow.Inspections.GetByBeehiveIdAsync(beehiveId);
        return _mapper.Map<IEnumerable<InspectionDto>>(inspections);
    }

    public async Task<InspectionDto> GetByIdAsync(int id)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        await _access.EnsureCanAccessBeehiveAsync(inspection.BeehiveId);

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task<InspectionDto> CreateAsync(CreateInspectionDto dto)
    {
        var beehive = await _uow.Beehives.GetByIdAsync(dto.BeehiveId)
            ?? throw new NotFoundException(nameof(Beehive), dto.BeehiveId);

        await _access.EnsureCanAccessBeehiveAsync(dto.BeehiveId);

        var inspection = _mapper.Map<Inspection>(dto);

        // Auto-populate temperature from the apiary's current weather.
        // Best-effort — a weather API failure never blocks saving an inspection.
        inspection.Temperature = await FetchCurrentTemperatureAsync(beehive.ApiaryId);

        await _uow.Inspections.AddAsync(inspection);
        await _uow.SaveChangesAsync();

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task<InspectionDto> UpdateAsync(int id, UpdateInspectionDto dto)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        await _access.EnsureCanAccessBeehiveAsync(inspection.BeehiveId);

        if (!await _uow.Beehives.ExistsAsync(dto.BeehiveId))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId);

        if (dto.BeehiveId != inspection.BeehiveId)
            await _access.EnsureCanAccessBeehiveAsync(dto.BeehiveId);

        // Preserve the temperature that was captured automatically on creation.
        // The field is no longer user-editable so the DTO will carry null.
        var originalTemperature = inspection.Temperature;
        _mapper.Map(dto, inspection);
        inspection.Temperature = originalTemperature;
        inspection.UpdatedAt = DateTime.UtcNow;

        await _uow.Inspections.UpdateAsync(inspection);
        await _uow.SaveChangesAsync();

        return _mapper.Map<InspectionDto>(inspection);
    }

    public async Task DeleteAsync(int id)
    {
        var inspection = await _uow.Inspections.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Inspection), id);

        await _access.EnsureCanAccessBeehiveAsync(inspection.BeehiveId);

        await _uow.Inspections.DeleteAsync(inspection);
        await _uow.SaveChangesAsync();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<double?> FetchCurrentTemperatureAsync(int apiaryId)
    {
        try
        {
            var apiary = await _uow.Apiaries.GetByIdAsync(apiaryId);
            if (apiary?.Latitude is null || apiary.Longitude is null)
                return null;

            return await _weather.GetCurrentTemperatureAsync(
                apiary.Latitude.Value, apiary.Longitude.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not fetch current temperature for apiary {ApiaryId} — inspection saved without temperature", apiaryId);
            return null;
        }
    }
}
