using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.OrgManagement.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.OrgManagement;

// ── Interface ────────────────────────────────────────────────────────────────

public interface IOrgManagementService
{
    /// <summary>Returns all manageable members in the organization (User and Admin roles only).</summary>
    Task<IEnumerable<OrgMemberDto>> GetMembersAsync(int orgId);

    /// <summary>Returns a single member with their current assignments.</summary>
    Task<OrgMemberDto> GetMemberAsync(int memberId, int orgId);

    /// <summary>
    /// Updates beehive assignments for a User-role member.
    /// Admin callers are restricted to beehives within their own apiary.
    /// </summary>
    Task<OrgMemberDto> UpdateBeehiveAssignmentsAsync(
        int memberId, UpdateBeehiveAssignmentsDto dto,
        int callerOrgId, int? callerApiaryId, string callerRole);

    /// <summary>Updates the apiary assignment for an Admin-role member. OrgAdmin only.</summary>
    Task<OrgMemberDto> UpdateApiaryAssignmentAsync(int memberId, UpdateApiaryAssignmentDto dto, int callerOrgId);

    /// <summary>
    /// Returns beehives available for assignment.
    /// OrgAdmin gets all org beehives; Admin gets only their apiary's beehives.
    /// </summary>
    Task<IEnumerable<OrgAvailableBeehiveDto>> GetAvailableBeehivesAsync(int orgId, int? callerApiaryId);

    /// <summary>Returns all apiaries in the organization for assigning to Admin users. OrgAdmin only.</summary>
    Task<IEnumerable<OrgAvailableApiaryDto>> GetAvailableApiariesAsync(int orgId);
}

// ── Implementation ───────────────────────────────────────────────────────────

public class OrgManagementService : IOrgManagementService
{
    private readonly IUnitOfWork _uow;

    public OrgManagementService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<OrgMemberDto>> GetMembersAsync(int orgId)
    {
        var users = await _uow.Users.GetAllWithOrganizationAsync();
        var orgUsers = users.Where(u =>
            u.OrganizationId == orgId &&
            u.Role is UserRole.User or UserRole.Admin);

        var dtos = new List<OrgMemberDto>();
        foreach (var u in orgUsers)
        {
            var withBeehives = u.Role == UserRole.User
                ? await _uow.Users.GetByIdWithAssignedBeehivesAsync(u.Id)
                : u;
            dtos.Add(MapMember(withBeehives ?? u));
        }
        return dtos;
    }

    public async Task<OrgMemberDto> GetMemberAsync(int memberId, int orgId)
    {
        var user = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (user.OrganizationId != orgId)
            throw new BusinessRuleException("This member does not belong to your organization.");

        if (user.Role is not (UserRole.User or UserRole.Admin))
            throw new BusinessRuleException("Only User and Admin role members can be managed here.");

        return MapMember(user);
    }

    public async Task<OrgMemberDto> UpdateBeehiveAssignmentsAsync(
        int memberId, UpdateBeehiveAssignmentsDto dto,
        int callerOrgId, int? callerApiaryId, string callerRole)
    {
        var member = await _uow.Users.GetByIdWithOrganizationAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (member.OrganizationId != callerOrgId)
            throw new BusinessRuleException("This member does not belong to your organization.");

        if (member.Role != UserRole.User)
            throw new BusinessRuleException("Beehive assignments can only be set for User-role members.");

        // Validate each requested beehive belongs to the org (and to caller's apiary if Admin)
        foreach (var beehiveId in dto.BeehiveIds)
        {
            var beehive = await _uow.Beehives.GetByIdAsync(beehiveId)
                ?? throw new NotFoundException(nameof(Beehive), beehiveId);

            var apiary = await _uow.Apiaries.GetByIdAsync(beehive.ApiaryId)
                ?? throw new NotFoundException(nameof(Apiary), beehive.ApiaryId);

            if (apiary.OrganizationId != callerOrgId)
                throw new BusinessRuleException($"Beehive '{beehive.Name}' does not belong to your organization.");

            if (callerRole == "Admin" && callerApiaryId.HasValue && beehive.ApiaryId != callerApiaryId.Value)
                throw new BusinessRuleException($"Beehive '{beehive.Name}' is not in your apiary.");
        }

        await _uow.Users.SetBeehiveAssignmentsAsync(memberId, dto.BeehiveIds);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId);
        return MapMember(updated!);
    }

    public async Task<OrgMemberDto> UpdateApiaryAssignmentAsync(
        int memberId, UpdateApiaryAssignmentDto dto, int callerOrgId)
    {
        var member = await _uow.Users.GetByIdWithOrganizationAsync(memberId)
            ?? throw new NotFoundException(nameof(User), memberId);

        if (member.OrganizationId != callerOrgId)
            throw new BusinessRuleException("This member does not belong to your organization.");

        if (member.Role != UserRole.Admin)
            throw new BusinessRuleException("Apiary assignment can only be set for Admin-role members.");

        if (dto.ApiaryId.HasValue)
        {
            var apiary = await _uow.Apiaries.GetByIdAsync(dto.ApiaryId.Value)
                ?? throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);

            if (apiary.OrganizationId != callerOrgId)
                throw new BusinessRuleException("The selected apiary does not belong to your organization.");
        }

        member.ApiaryId = dto.ApiaryId;
        await _uow.Users.UpdateAsync(member);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Users.GetByIdWithAssignedBeehivesAsync(memberId);
        return MapMember(updated!);
    }

    public async Task<IEnumerable<OrgAvailableBeehiveDto>> GetAvailableBeehivesAsync(int orgId, int? callerApiaryId)
    {
        var beehives = await _uow.Beehives.GetByOrganizationAsync(orgId);

        if (callerApiaryId.HasValue)
            beehives = beehives.Where(b => b.ApiaryId == callerApiaryId.Value);

        return beehives.Select(b => new OrgAvailableBeehiveDto
        {
            Id = b.Id,
            Name = b.Name,
            ApiaryName = b.Apiary?.Name ?? string.Empty,
        });
    }

    public async Task<IEnumerable<OrgAvailableApiaryDto>> GetAvailableApiariesAsync(int orgId)
    {
        var apiaries = await _uow.Apiaries.GetAllByOrganizationAsync(orgId);
        return apiaries.Select(a => new OrgAvailableApiaryDto { Id = a.Id, Name = a.Name });
    }

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
