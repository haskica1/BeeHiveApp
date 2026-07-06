using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Security;
using BeeHive.Application.Common.Services;
using BeeHive.Application.Features.Beehives.DTOs;
using BeeHive.Application.Features.Notifications;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeeHive.Application.Features.Beehives;

public class BeehiveService : IBeehiveService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IQrCodeService _qr;
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _currentUser;
    private readonly IAccessGuard _access;
    private readonly IPlanGuard _plan;
    private readonly ILogger<BeehiveService> _logger;
    private readonly string _frontendUrl;

    public BeehiveService(
        IUnitOfWork uow,
        IMapper mapper,
        IQrCodeService qr,
        INotificationService notifications,
        ICurrentUser currentUser,
        IAccessGuard access,
        IPlanGuard plan,
        ILogger<BeehiveService> logger,
        IConfiguration config)
    {
        _uow           = uow;
        _mapper        = mapper;
        _qr            = qr;
        _notifications = notifications;
        _currentUser   = currentUser;
        _access        = access;
        _plan          = plan;
        _logger        = logger;
        _frontendUrl   = config["FrontendUrl"] ?? "https://bee-hive-app.vercel.app";
    }

    public async Task<IEnumerable<BeehiveDto>> GetByApiaryIdAsync(int apiaryId)
    {
        if (!await _uow.Apiaries.ExistsAsync(apiaryId))
            throw new NotFoundException(nameof(Apiary), apiaryId);

        // Managers must own the apiary; a Beekeeper is filtered to assigned hives below.
        if (_currentUser.Role != UserRole.Beekeeper)
            await _access.EnsureCanManageApiaryAsync(apiaryId);

        var beehives = await _uow.Beehives.GetByApiaryIdAsync(apiaryId);
        var inspectionCounts = await _uow.Inspections.CountByBeehiveForApiaryAsync(apiaryId);

        if (_currentUser.Role == UserRole.Beekeeper)
        {
            var assignedIds = await _access.GetAssignedBeehiveIdsAsync();
            beehives = beehives.Where(b => assignedIds.Contains(b.Id)).ToList();
        }

        return beehives.Select(b =>
        {
            var dto = _mapper.Map<BeehiveDto>(b);
            dto.InspectionCount = inspectionCounts.GetValueOrDefault(b.Id);
            return dto;
        }).ToList();
    }

    public async Task<BeehiveDetailDto> GetByIdAsync(int id)
    {
        var beehive = await _uow.Beehives.GetWithInspectionsAsync(id)
            ?? throw new NotFoundException(nameof(Beehive), id);

        await _access.EnsureCanAccessBeehiveAsync(id);

        return _mapper.Map<BeehiveDetailDto>(beehive);
    }

    public async Task<BeehiveDto> CreateAsync(CreateBeehiveDto dto)
    {
        var apiary = await _uow.Apiaries.GetByIdAsync(dto.ApiaryId)
            ?? throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        await _access.EnsureCanManageApiaryAsync(dto.ApiaryId);
        await _plan.EnsureCanAddBeehiveAsync(apiary.OrganizationId);

        var beehive = _mapper.Map<Beehive>(dto);
        beehive.CreatedById  = _currentUser.UserId;
        beehive.UniqueId     = Guid.NewGuid();
        beehive.QrCodeBase64 = _qr.GeneratePngBase64($"{_frontendUrl}/scan/{beehive.UniqueId}");

        await _uow.Beehives.AddAsync(beehive);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Beehives.GetWithInspectionsAsync(beehive.Id) ?? beehive;

        // Notify the creator's superior about the new beehive.
        if (_currentUser.UserId is int creatorId)
        {
            var creator = await _uow.Users.GetByIdWithOrganizationAsync(creatorId);
            if (creator != null)
                await SendBeehiveCreatedNotificationsAsync(saved, creator);
        }

        return _mapper.Map<BeehiveDto>(saved);
    }

    public async Task<BeehiveDto> UpdateAsync(int id, UpdateBeehiveDto dto)
    {
        var beehive = await _uow.Beehives.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Beehive), id);

        // Must be able to manage the beehive's current apiary…
        await _access.EnsureCanManageApiaryAsync(beehive.ApiaryId);

        if (!await _uow.Apiaries.ExistsAsync(dto.ApiaryId))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId);

        // …and the target apiary, in case the beehive is being moved.
        if (dto.ApiaryId != beehive.ApiaryId)
            await _access.EnsureCanManageApiaryAsync(dto.ApiaryId);

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

        await _access.EnsureCanManageApiaryAsync(beehive.ApiaryId);

        await _uow.Beehives.DeleteAsync(beehive);
        await _uow.SaveChangesAsync();
    }

    public async Task<BeehiveScanDto?> GetScanInfoAsync(Guid uniqueId)
    {
        var beehive = await _uow.Beehives.GetByUniqueIdAsync(uniqueId);
        if (beehive is null) return null;
        return new BeehiveScanDto { Id = beehive.Id, Name = beehive.Name, ApiaryId = beehive.ApiaryId };
    }

    public async Task<IEnumerable<BeehiveQrDto>> GetQrCodesByApiaryAsync(int apiaryId)
    {
        if (!await _uow.Apiaries.ExistsAsync(apiaryId))
            throw new NotFoundException(nameof(Apiary), apiaryId);

        if (_currentUser.Role != UserRole.Beekeeper)
            await _access.EnsureCanManageApiaryAsync(apiaryId);

        var beehives = await _uow.Beehives.FindAsync(b => b.ApiaryId == apiaryId);

        if (_currentUser.Role == UserRole.Beekeeper)
        {
            var assignedIds = await _access.GetAssignedBeehiveIdsAsync();
            beehives = beehives.Where(b => assignedIds.Contains(b.Id));
        }

        return beehives
            .OrderBy(b => b.Name)
            .Select(b => new BeehiveQrDto
            {
                Id           = b.Id,
                Name         = b.Name,
                UniqueId     = b.UniqueId,
                QrCodeBase64 = b.QrCodeBase64,
            })
            .ToList();
    }

    public Task<bool> CanCurrentUserAccessAsync(int beehiveId) =>
        _access.CanAccessBeehiveAsync(beehiveId);

    public async Task<IEnumerable<BeehiveDto>> GetAllForCurrentUserAsync()
    {
        IEnumerable<Beehive> beehives;

        if (_currentUser.Role == UserRole.SystemAdmin)
        {
            beehives = await _uow.Beehives.GetAllAsync();
        }
        else if (_currentUser.Role == UserRole.Beekeeper)
        {
            var assignedIds = await _access.GetAssignedBeehiveIdsAsync();
            beehives = assignedIds.Count > 0
                ? await _uow.Beehives.FindAsync(b => assignedIds.Contains(b.Id))
                : [];
        }
        else if (_currentUser.Role == UserRole.ApiaryAdmin && _currentUser.ApiaryId.HasValue)
        {
            beehives = await _uow.Beehives.GetByApiaryIdAsync(_currentUser.ApiaryId.Value);
        }
        else if (_currentUser.OrganizationId.HasValue)
        {
            beehives = await _uow.Beehives.GetByOrganizationAsync(_currentUser.OrganizationId.Value);
        }
        else
        {
            beehives = [];
        }

        return _mapper.Map<IEnumerable<BeehiveDto>>(beehives);
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

        if (creator.Role == UserRole.ApiaryAdmin)
        {
            // Use apiary.OrganizationId (more reliable than creator.OrganizationId)
            var orgAdmins = await _uow.Users.FindAsync(u =>
                u.OrganizationId == apiary.OrganizationId && u.Role == UserRole.OrganizationAdmin);

            foreach (var orgAdmin in orgAdmins)
            {
                await _notifications.NotifyAsync(
                    orgAdmin.Id,
                    "Nova košnica",
                    $"Admin {creator.FirstName} {creator.LastName} je dodao/la košnicu '{beehive.Name}' u pčelinjak '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
        else if (creator.Role == UserRole.OrganizationAdmin)
        {
            var admins = await _uow.Users.FindAsync(u =>
                u.ApiaryId == beehive.ApiaryId && u.Role == UserRole.ApiaryAdmin);

            foreach (var admin in admins)
            {
                await _notifications.NotifyAsync(
                    admin.Id,
                    "Nova košnica",
                    $"Administrator organizacije {creator.FirstName} {creator.LastName} je dodao/la košnicu '{beehive.Name}' u vaš pčelinjak '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
        else if (creator.Role == UserRole.SystemAdmin)
        {
            var orgAdmins = await _uow.Users.FindAsync(u =>
                u.OrganizationId == apiary.OrganizationId && u.Role == UserRole.OrganizationAdmin);

            foreach (var orgAdmin in orgAdmins)
            {
                await _notifications.NotifyAsync(
                    orgAdmin.Id,
                    "Nova košnica",
                    $"Sistemski administrator je dodao košnicu '{beehive.Name}' u pčelinjak '{apiary.Name}'.",
                    NotificationType.BeehiveCreated,
                    beehive.Id, nameof(Beehive));
            }
        }
    }
}
