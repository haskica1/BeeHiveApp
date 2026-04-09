using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Apiaries;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IApiaryService
{
    Task<IEnumerable<ApiaryDto>> GetAllAsync();
    Task<ApiaryDetailDto> GetByIdAsync(int id);
    Task<ApiaryDto> CreateAsync(CreateApiaryDto dto);
    Task<ApiaryDto> UpdateAsync(int id, UpdateApiaryDto dto);
    Task DeleteAsync(int id);
}

// ── Implementation ───────────────────────────────────────────────────────────

public class ApiaryService : IApiaryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ApiaryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApiaryDto>> GetAllAsync()
    {
        var apiaries = await _uow.Apiaries.GetAllWithBeehivesAsync();
        return _mapper.Map<IEnumerable<ApiaryDto>>(apiaries);
    }

    /// <inheritdoc />
    public async Task<ApiaryDetailDto> GetByIdAsync(int id)
    {
        var apiary = await _uow.Apiaries.GetWithBeehivesAsync(id)
            ?? throw new NotFoundException(nameof(Apiary), id);

        return _mapper.Map<ApiaryDetailDto>(apiary);
    }

    /// <inheritdoc />
    public async Task<ApiaryDto> CreateAsync(CreateApiaryDto dto)
    {
        var apiary = _mapper.Map<Apiary>(dto);
        await _uow.Apiaries.AddAsync(apiary);
        await _uow.SaveChangesAsync();
        return _mapper.Map<ApiaryDto>(apiary);
    }

    /// <inheritdoc />
    public async Task<ApiaryDto> UpdateAsync(int id, UpdateApiaryDto dto)
    {
        var apiary = await _uow.Apiaries.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Apiary), id);

        _mapper.Map(dto, apiary);
        apiary.UpdatedAt = DateTime.UtcNow;

        await _uow.Apiaries.UpdateAsync(apiary);
        await _uow.SaveChangesAsync();

        return _mapper.Map<ApiaryDto>(apiary);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id)
    {
        var apiary = await _uow.Apiaries.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Apiary), id);

        await _uow.Apiaries.DeleteAsync(apiary);
        await _uow.SaveChangesAsync();
    }
}
