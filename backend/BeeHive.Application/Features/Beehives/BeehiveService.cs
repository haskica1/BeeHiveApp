using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Services;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Notifications;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

    /// <summary>Returns the set of beehive IDs assigned to the given user.</summary>
    Task<HashSet<int>> GetAssignedBeehiveIdsAsync(int userId);

    /// <summary>Returns the set of apiary IDs that contain at least one beehive assigned to the given user.</summary>
    Task<HashSet<int>> GetAssignedApiaryIdsAsync(int userId);

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
    private readonly INotificationService _notifications;
    private readonly ILogger<BeehiveService> _logger;
    private readonly string _frontendUrl;

    public BeehiveService(
        IUnitOfWork uow,
        IMapper mapper,
        IQrCodeService qr,
        INotificationService notifications,
        ILogger<BeehiveService> logger,
        IConfiguration config)
    {
        _uow           = uow;
        _mapper        = mapper;
        _qr            = qr;
        _notifications = notifications;
        _logger        = logger;
        _frontendUrl   = config["FrontendUrl"] ?? "https://bee-hive-app.vercel.app";
    }

    public async Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId)
    {
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
        if (!await _uow.Apiaries.ExistsAsync(dto.ApiaryId))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        var beehive = _mapper.Map<Beehive>(dto);
        beehive.CreatedById = createdById;
        beehive.UniqueId     = Guid.NewGuid();
        beehive.QrCodeBase64 = _qr.GeneratePngBase64($"{_frontendUrl}/scan/{beehive.UniqueId}");

        await _uow.Beehives.AddAsync(beehive);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Beehives.GetWithInspectionsAsync(beehive.Id) ?? beehive;

        // 5) Beehive creation notifications — notify the creator's superior
        if (createdById.HasValue)
        {
            var creator = await _uow.Users.GetByIdWithOrganizationAsync(createdById.Value);
            if (creator != null)
                await SendBeehiveCreatedNotificationsAsync(saved, creator);
        }

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

    public async Task<HashSet<int>> GetAssignedBeehiveIdsAsync(int userId)
    {
        var user = await _uow.Users.GetByIdWithAssignedBeehivesAsync(userId);
        if (user is null) return [];
        return user.AssignedBeehives.Select(ub => ub.BeehiveId).ToHashSet();
    }

    public async Task<HashSet<int>> GetAssignedApiaryIdsAsync(int userId)
    {
        var user = await _uow.Users.GetByIdWithAssignedBeehivesAsync(userId);
        if (user is null) return [];
        return user.AssignedBeehives.Select(ub => ub.Beehive.ApiaryId).ToHashSet();
    }

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

    // ── Notification helpers ──────────────────────────────────────────────────

    private async Task SendBeehiveCreatedNotificationsAsync(Beehive beehive, User creator)
    {
        var apiary = await _uow.Apiaries.GetByIdAsync(beehive.ApiaryId);
        if (apiary == null)
        {
            _logger.LogWarning("SendBeehiveCreatedNotifications: apiary {ApiaryId} not found — skipping", beehive.ApiaryId);
            return;
        }

        _logger.LogInformation(
            "SendBeehiveCreatedNotifications: beehive={BeehiveId} creator={CreatorId} role={Role} apiary.OrgId={OrgId}",
            beehive.Id, creator.Id, creator.Role, apiary.OrganizationId);

        if (creator.Role == UserRole.Admin)
        {
            // Use apiary.OrganizationId (more reliable than creator.OrganizationId)
            var orgAdmins = await _uow.Users.FindAsync(u =>
                u.OrganizationId == apiary.OrganizationId && u.Role == UserRole.OrgAdmin);

            _logger.LogInformation("SendBeehiveCreatedNotifications: Admin path — found {Count} OrgAdmins to notify", orgAdmins.Count());

            foreach (var orgAdmin in orgAdmins)
            {
                await _notifications.NotifyAsync(
                    orgAdmin.Id,
                    "New beehive created",
                    $"Admin {creator.FirstName} {creator.LastName} created beehive '{beehive.Name}' in apiary '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
        else if (creator.Role == UserRole.OrgAdmin)
        {
            var admins = await _uow.Users.FindAsync(u =>
                u.ApiaryId == beehive.ApiaryId && u.Role == UserRole.Admin);

            _logger.LogInformation("SendBeehiveCreatedNotifications: OrgAdmin path — found {Count} Admins to notify", admins.Count());

            foreach (var admin in admins)
            {
                await _notifications.NotifyAsync(
                    admin.Id,
                    "New beehive created",
                    $"Organization Admin {creator.FirstName} {creator.LastName} created beehive '{beehive.Name}' in your apiary '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
        else if (creator.Role == UserRole.SystemAdmin)
        {
            var orgAdmins = await _uow.Users.FindAsync(u =>
                u.OrganizationId == apiary.OrganizationId && u.Role == UserRole.OrgAdmin);

            _logger.LogInformation("SendBeehiveCreatedNotifications: SystemAdmin path — found {Count} OrgAdmins to notify", orgAdmins.Count());

            foreach (var orgAdmin in orgAdmins)
            {
                await _notifications.NotifyAsync(
                    orgAdmin.Id,
                    "New beehive created",
                    $"System Admin created beehive '{beehive.Name}' in apiary '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
    }
}
