using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Notifications;
using BeeHive.Application.Features.OrgManagement.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.OrgManagement;

public class OrgManagementService : IOrgManagementService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notifications;
    private readonly ICurrentUser _currentUser;

    public OrgManagementService(IUnitOfWork uow, INotificationService notifications, ICurrentUser currentUser)
    {
        _uow           = uow;
        _notifications = notifications;
        _currentUser   = currentUser;
    }

    public async Task<IEnumerable<OrgMemberDto>> GetMembersAsync()
    {
        if (_currentUser.OrganizationId is not int orgId)
            return [];

        var users = await _uow.Users.GetAllWithOrganizationAsync();
        var orgUsers = users.Where(u =>
            u.OrganizationId == orgId &&
            u.Role is UserRole.Beekeeper or UserRole.ApiaryAdmin);

        var dtos = new List<OrgMemberDto>();
        foreach (var u in orgUsers)
        {
            var withBeehives = u.Role == UserRole.Beekeeper
                ? await _uow.Users.GetByIdWithAssignedBeehivesAsync(u.Id)
                : u;
            dtos.Add(MapMember(withBeehives ?? u));
        }
        return dtos;
    }

    public async Task<OrgMemberDto> GetMemberAsync(int memberId)
    {
        var orgId = RequireOrganization();

        var user = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (user.OrganizationId != orgId)
            throw new ForbiddenAccessException("This member does not belong to your organization.");

        if (user.Role is not (UserRole.Beekeeper or UserRole.ApiaryAdmin))
            throw new BusinessRuleException("Only User and Admin role members can be managed here.");

        return MapMember(user);
    }

    public async Task<OrgMemberDto> UpdateBeehiveAssignmentsAsync(int memberId, UpdateBeehiveAssignmentsDto dto)
    {
        var orgId = RequireOrganization();

        var member = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (member.OrganizationId != orgId)
            throw new ForbiddenAccessException("This member does not belong to your organization.");

        if (member.Role != UserRole.Beekeeper)
            throw new BusinessRuleException("Beehive assignments can only be set for User-role members.");

        // Validate each requested beehive belongs to the org (and to caller's apiary if ApiaryAdmin)
        foreach (var beehiveId in dto.BeehiveIds)
        {
            var beehive = await _uow.Beehives.GetByIdAsync(beehiveId)
                ?? throw new NotFoundException(nameof(Beehive), beehiveId);

            var apiary = await _uow.Apiaries.GetByIdAsync(beehive.ApiaryId)
                ?? throw new NotFoundException(nameof(Apiary), beehive.ApiaryId);

            if (apiary.OrganizationId != orgId)
                throw new BusinessRuleException($"Beehive '{beehive.Name}' does not belong to your organization.");

            if (_currentUser.Role == UserRole.ApiaryAdmin && _currentUser.ApiaryId is int callerApiaryId
                && beehive.ApiaryId != callerApiaryId)
                throw new BusinessRuleException($"Beehive '{beehive.Name}' is not in your apiary.");
        }

        // Capture old assignments for change detection
        var oldBeehiveIds = member.AssignedBeehives.Select(ub => ub.BeehiveId).ToHashSet();
        var newBeehiveIds = dto.BeehiveIds.ToHashSet();

        await _uow.Users.SetBeehiveAssignmentsAsync(memberId, dto.BeehiveIds);
        await _uow.SaveChangesAsync();

        // Notify member of beehive assignment changes
        foreach (var beehiveId in newBeehiveIds.Except(oldBeehiveIds))
        {
            var beehive = await _uow.Beehives.GetByIdAsync(beehiveId);
            if (beehive == null) continue;
            await _notifications.NotifyAsync(
                memberId,
                "Beehive assigned",
                $"You have been assigned to beehive '{beehive.Name}'.",
                NotificationType.BeehiveAssigned,
                beehiveId, nameof(Beehive));
        }

        foreach (var beehiveId in oldBeehiveIds.Except(newBeehiveIds))
        {
            var beehive = await _uow.Beehives.GetByIdAsync(beehiveId);
            await _notifications.NotifyAsync(
                memberId,
                "Beehive unassigned",
                $"You have been unassigned from beehive '{beehive?.Name ?? "Unknown"}'.",
                NotificationType.BeehiveUnassigned,
                beehiveId, nameof(Beehive));
        }

        var updated = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId);
        return MapMember(updated!);
    }

    public async Task<OrgMemberDto> UpdateApiaryAssignmentAsync(int memberId, UpdateApiaryAssignmentDto dto)
    {
        var orgId = RequireOrganization();

        var member = await _uow.Users.GetByIdWithOrganizationAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (member.OrganizationId != orgId)
            throw new ForbiddenAccessException("This member does not belong to your organization.");

        if (member.Role != UserRole.ApiaryAdmin)
            throw new BusinessRuleException("Apiary assignment can only be set for Admin-role members.");

        Apiary? newApiary = null;
        if (dto.ApiaryId.HasValue)
        {
            newApiary = await _uow.Apiaries.GetByIdAsync(dto.ApiaryId.Value)
                ?? throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);

            if (newApiary.OrganizationId != orgId)
                throw new BusinessRuleException("The selected apiary does not belong to your organization.");
        }

        var oldApiaryId = member.ApiaryId;
        member.ApiaryId = dto.ApiaryId;
        await _uow.Users.UpdateAsync(member);
        await _uow.SaveChangesAsync();

        // Notify admin of apiary assignment change
        if (dto.ApiaryId.HasValue && dto.ApiaryId != oldApiaryId && newApiary != null)
        {
            await _notifications.NotifyAsync(
                memberId,
                "Apiary assigned",
                $"You have been assigned as Admin for apiary '{newApiary.Name}'.",
                NotificationType.ApiaryAssigned,
                dto.ApiaryId.Value, nameof(Apiary));
        }
        else if (!dto.ApiaryId.HasValue && oldApiaryId.HasValue)
        {
            await _notifications.NotifyAsync(
                memberId,
                "Apiary unassigned",
                "You have been unassigned from your apiary.",
                NotificationType.ApiaryUnassigned);
        }

        var updated = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId);
        return MapMember(updated!);
    }

    public async Task<IEnumerable<OrgAvailableBeehiveDto>> GetAvailableBeehivesAsync()
    {
        if (_currentUser.OrganizationId is not int orgId)
            return [];

        var beehives = await _uow.Beehives.GetByOrganizationAsync(orgId);

        // An ApiaryAdmin may only assign beehives from their own apiary.
        if (_currentUser.Role == UserRole.ApiaryAdmin && _currentUser.ApiaryId is int callerApiaryId)
            beehives = beehives.Where(b => b.ApiaryId == callerApiaryId);

        return beehives.Select(b => new OrgAvailableBeehiveDto
        {
            Id = b.Id,
            Name = b.Name,
            ApiaryName = b.Apiary?.Name ?? string.Empty,
        });
    }

    public async Task<IEnumerable<OrgAvailableApiaryDto>> GetAvailableApiariesAsync()
    {
        if (_currentUser.OrganizationId is not int orgId)
            return [];

        var apiaries = await _uow.Apiaries.GetAllByOrganizationAsync(orgId);
        return apiaries.Select(a => new OrgAvailableApiaryDto { Id = a.Id, Name = a.Name });
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private int RequireOrganization() =>
        _currentUser.OrganizationId
            ?? throw new ForbiddenAccessException("You must belong to an organization to manage its members.");

    private static OrgMemberDto MapMember(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.Role.ToString(),
        ApiaryId = u.ApiaryId,
        ApiaryName = u.Apiary?.Name,
        AssignedBeehiveIds = u.AssignedBeehives.Select(ub => ub.BeehiveId).ToList(),
        AssignedBeehiveNames = u.AssignedBeehives
            .Select(ub => ub.Beehive?.Name ?? string.Empty)
            .Where(n => n != string.Empty)
            .ToList(),
    };
}
