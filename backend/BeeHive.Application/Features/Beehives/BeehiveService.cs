using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Services;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace BeeHive.Application.Features.Beehives;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IBeehiveService
{
    Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId);
    Task<BeehiveDetailDto> GetByIdAsync(int id);
    Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto, int? createdById);
    Task<BeehiveDto> UpdateAsync(int id, UpdateBeehiveDto dto);
    Task DeleteAsync(int id);
    Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId);

    /// <summary>Public scan lookup — resolves a uniqueId to the minimal beehive info needed for redirect.</summary>
    Task<BeehiveScanDto?> GetScanInfoAsync(Guid uniqueId);

    /// <summary>Checks whether the given user has access to view the beehive based on their role.</summary>
    Task<bool> HasAccessAsync(int userId, string role, string? apiaryIdClaim, int beehiveId);

    /// <summary>Regenerates QR codes for all beehives using the current scan URL format. Returns count updated.</summary>
    Task<int> RegenerateAllQrCodesAsync();
}

// ── Implementation ───────────────────────────────────────────────────────────

public class BeehiveService : IBeehiveService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IQrCodeService _qr;
    private readonly string _frontendUrl;

    public BeehiveService(IUnitOfWork uow, IMapper mapper, IQrCodeService qr, IConfiguration config)
    {
        _uow         = uow;
        _mapper      = mapper;
        _qr          = qr;
        _frontendUrl = config["FrontendUrl"] ?? "https://bee-hive-app.vercel.app";
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

    public async Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto, int? createdById)
    {
        // Ensure the target apiary exists before creating the beehive
        if (!await _uow.Apiaries.ExistsAsync(dto.ApiaryId))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        var beehive = _mapper.Map<Beehive>(dto);
        beehive.CreatedById = createdById;

        // Assign a permanent unique ID and generate the scan QR code once, on creation
        beehive.UniqueId     = Guid.NewGuid();
        beehive.QrCodeBase64 = _qr.GeneratePngBase64($"{_frontendUrl}/scan/{beehive.UniqueId}");

        await _uow.Beehives.AddAsync(beehive);
        await _uow.SaveChangesAsync();

        // Reload to get CreatedBy nav property
        var saved = await _uow.Beehives.GetWithInspectionsAsync(beehive.Id) ?? beehive;
        return _mapper.Map<BeehiveDto>(saved);
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

    public Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId) =>
        _uow.Users.IsUserAssignedToBeehiveAsync(userId, beehiveId);

    public async Task<BeehiveScanDto?> GetScanInfoAsync(Guid uniqueId)
    {
        var beehive = await _uow.Beehives.GetByUniqueIdAsync(uniqueId);
        if (beehive is null) return null;
        return new BeehiveScanDto { Id = beehive.Id, Name = beehive.Name, ApiaryId = beehive.ApiaryId };
    }

    public async Task<bool> HasAccessAsync(int userId, string role, string? apiaryIdClaim, int beehiveId)
    {
        return role switch
        {
            "SystemAdmin" or "OrgAdmin" => true,
            "Admin" when apiaryIdClaim != null && int.TryParse(apiaryIdClaim, out var adminApiaryId) =>
                await _uow.Beehives.GetByIdAsync(beehiveId) is { } b && b.ApiaryId == adminApiaryId,
            "User" => await _uow.Users.IsUserAssignedToBeehiveAsync(userId, beehiveId),
            _ => false,
        };
    }

    public async Task<int> RegenerateAllQrCodesAsync()
    {
        var beehives = await _uow.Beehives.GetAllWithUniqueIdAsync();
        int count = 0;
        foreach (var b in beehives)
        {
            b.QrCodeBase64 = _qr.GeneratePngBase64($"{_frontendUrl}/scan/{b.UniqueId}");
            await _uow.Beehives.UpdateAsync(b);
            count++;
        }
        await _uow.SaveChangesAsync();
        return count;
    }
}
