using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Admin.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Admin;

public interface IAdminService
{
    // Organizations
    Task<IEnumerable<AdminOrganizationDto>> GetAllOrganizationsAsync();
    Task<AdminOrganizationDto> GetOrganizationByIdAsync(int id);
    Task<AdminOrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, int? createdById);
    Task<AdminOrganizationDto> UpdateOrganizationAsync(int id, UpdateOrganizationDto dto);
    Task DeleteOrganizationAsync(int id);

    // Apiaries (for org-scoped picker)
    Task<IEnumerable<AdminApiaryListItemDto>> GetApiariesByOrganizationAsync(int organizationId);

    // Beehives (for org-scoped beehive picker when assigning User role)
    Task<IEnumerable<AdminBeehiveListItemDto>> GetBeehivesByOrganizationAsync(int organizationId);

    // Users
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<AdminUserDto> GetUserByIdAsync(int id);
    Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto);
    Task<AdminUserDto> UpdateUserAsync(int id, UpdateAdminUserDto dto);
    Task DeleteUserAsync(int id);
}

public class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;

    public AdminService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ── Organizations ──────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminOrganizationDto>> GetAllOrganizationsAsync()
    {
        var orgs = await _uow.Organizations.GetAllWithDetailsAsync();
        return orgs.Select(MapOrganization);
    }

    public async Task<AdminOrganizationDto> GetOrganizationByIdAsync(int id)
    {
        var org = await _uow.Organizations.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Organization), id);
        return MapOrganization(org);
    }

    public async Task<AdminOrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, int? createdById)
    {
        var org = new Organization
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedById = createdById,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Organizations.AddAsync(org);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Organizations.GetWithDetailsAsync(org.Id) ?? org;
        return MapOrganization(saved);
    }

    public async Task<AdminOrganizationDto> UpdateOrganizationAsync(int id, UpdateOrganizationDto dto)
    {
        var org = await _uow.Organizations.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Organization), id);

        org.Name = dto.Name.Trim();
        org.Description = dto.Description?.Trim();

        await _uow.Organizations.UpdateAsync(org);
        await _uow.SaveChangesAsync();

        return MapOrganization(org);
    }

    public async Task DeleteOrganizationAsync(int id)
    {
        var org = await _uow.Organizations.GetWithDetailsAsync(id)
            ?? throw new NotFoundException(nameof(Organization), id);

        if (org.Users.Count > 0)
            throw new BusinessRuleException(
                $"Cannot delete organization '{org.Name}' because it has {org.Users.Count} user(s). Remove or reassign users first.");

        await _uow.Organizations.DeleteAsync(org);
        await _uow.SaveChangesAsync();
    }

    // ── Apiaries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminApiaryListItemDto>> GetApiariesByOrganizationAsync(int organizationId)
    {
        var apiaries = await _uow.Apiaries.GetAllByOrganizationAsync(organizationId);
        return apiaries.Select(a => new AdminApiaryListItemDto { Id = a.Id, Name = a.Name });
    }

    public async Task<IEnumerable<AdminBeehiveListItemDto>> GetBeehivesByOrganizationAsync(int organizationId)
    {
        var beehives = await _uow.Beehives.GetByOrganizationAsync(organizationId);
        return beehives.Select(b => new AdminBeehiveListItemDto
        {
            Id = b.Id,
            Name = b.Name,
            ApiaryName = b.Apiary?.Name ?? string.Empty,
        });
    }

    // ── Users ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _uow.Users.GetAllWithOrganizationAsync();
        var dtos = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var withBeehives = u.Role == UserRole.User
                ? await _uow.Users.GetByIdWithAssignedBeehivesAsync(u.Id)
                : u;
            dtos.Add(MapUser(withBeehives ?? u));
        }
        return dtos;
    }

    public async Task<AdminUserDto> GetUserByIdAsync(int id)
    {
        var user = await _uow.Users.GetByIdWithAssignedBeehivesAsync(id)
            ?? throw new NotFoundException(nameof(User), id);
        return MapUser(user);
    }

    public async Task<AdminUserDto> CreateUserAsync(CreateAdminUserDto dto)
    {
        var existing = await _uow.Users.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (existing != null)
            throw new BusinessRuleException($"A user with email '{dto.Email}' already exists.");

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new BusinessRuleException($"Invalid role '{dto.Role}'.");

        ValidateRoleOrgApiaryConsistency(role, dto.OrganizationId, dto.ApiaryId);

        if (dto.OrganizationId.HasValue)
        {
            var orgExists = await _uow.Organizations.ExistsAsync(dto.OrganizationId.Value);
            if (!orgExists)
                throw new NotFoundException(nameof(Organization), dto.OrganizationId.Value);
        }

        if (dto.ApiaryId.HasValue)
        {
            var apiary = await _uow.Apiaries.GetByIdAsync(dto.ApiaryId.Value);
            if (apiary == null)
                throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);
            if (dto.OrganizationId.HasValue && apiary.OrganizationId != dto.OrganizationId.Value)
                throw new BusinessRuleException("The selected apiary does not belong to the selected organization.");
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            OrganizationId = dto.OrganizationId,
            ApiaryId = dto.ApiaryId,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        if (role == UserRole.User && dto.AssignedBeehiveIds.Count > 0)
        {
            await _uow.Users.SetBeehiveAssignmentsAsync(user.Id, dto.AssignedBeehiveIds);
            await _uow.SaveChangesAsync();
        }

        var created = await _uow.Users.GetByIdWithAssignedBeehivesAsync(user.Id);
        return MapUser(created!);
    }

    public async Task<AdminUserDto> UpdateUserAsync(int id, UpdateAdminUserDto dto)
    {
        var user = await _uow.Users.GetByIdWithOrganizationAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new BusinessRuleException($"Invalid role '{dto.Role}'.");

        ValidateRoleOrgApiaryConsistency(role, dto.OrganizationId, dto.ApiaryId);

        if (dto.OrganizationId.HasValue)
        {
            var orgExists = await _uow.Organizations.ExistsAsync(dto.OrganizationId.Value);
            if (!orgExists)
                throw new NotFoundException(nameof(Organization), dto.OrganizationId.Value);
        }

        if (dto.ApiaryId.HasValue)
        {
            var apiary = await _uow.Apiaries.GetByIdAsync(dto.ApiaryId.Value);
            if (apiary == null)
                throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);
            if (dto.OrganizationId.HasValue && apiary.OrganizationId != dto.OrganizationId.Value)
                throw new BusinessRuleException("The selected apiary does not belong to the selected organization.");
        }

        var newEmail = dto.Email.Trim().ToLower();
        if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var conflict = await _uow.Users.GetByEmailAsync(newEmail);
            if (conflict != null)
                throw new BusinessRuleException($"A user with email '{dto.Email}' already exists.");
        }

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = newEmail;
        user.Role = role;
        user.OrganizationId = dto.OrganizationId;
        user.ApiaryId = dto.ApiaryId;

        await _uow.Users.UpdateAsync(user);

        // Always sync beehive assignments: clear when not User role, set when User role
        await _uow.Users.SetBeehiveAssignmentsAsync(
            id,
            role == UserRole.User ? dto.AssignedBeehiveIds : []);

        await _uow.SaveChangesAsync();

        var updated = await _uow.Users.GetByIdWithAssignedBeehivesAsync(id);
        return MapUser(updated!);
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _uow.Users.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        if (user.Role == UserRole.SystemAdmin)
        {
            var allUsers = await _uow.Users.GetAllAsync();
            var adminCount = allUsers.Count(u => u.Role == UserRole.SystemAdmin);
            if (adminCount <= 1)
                throw new BusinessRuleException("Cannot delete the last SystemAdmin account.");
        }

        await _uow.Users.DeleteAsync(user);
        await _uow.SaveChangesAsync();
    }

    // ── Validation ─────────────────────────────────────────────────────────────

    private static void ValidateRoleOrgApiaryConsistency(UserRole role, int? organizationId, int? apiaryId)
    {
        if (role == UserRole.SystemAdmin)
        {
            if (organizationId != null)
                throw new BusinessRuleException("SystemAdmin users cannot belong to an organization.");
            return;
        }

        if (organizationId == null)
            throw new BusinessRuleException("Non-SystemAdmin users must belong to an organization.");

        if (role == UserRole.Admin && apiaryId == null)
            throw new BusinessRuleException("Admin users must be assigned to a specific apiary.");

        if (role != UserRole.Admin && apiaryId != null)
            throw new BusinessRuleException("Only Admin users can be assigned to a specific apiary.");
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static AdminOrganizationDto MapOrganization(Organization o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Description = o.Description,
        UserCount = o.Users.Count,
        ApiaryCount = o.Apiaries.Count,
        CreatedByName = o.CreatedBy != null ? $"{o.CreatedBy.FirstName} {o.CreatedBy.LastName}" : null,
        CreatedAt = o.CreatedAt
    };

    private static AdminUserDto MapUser(User u) => new()
    {
        Id = u.Id,
        FirstName = u.FirstName,
        LastName = u.LastName,
        Email = u.Email,
        Role = u.Role.ToString(),
        OrganizationId = u.OrganizationId,
        OrganizationName = u.Organization?.Name,
        ApiaryId = u.ApiaryId,
        ApiaryName = u.Apiary?.Name,
        AssignedBeehiveIds = u.AssignedBeehives.Select(ub => ub.BeehiveId).ToList(),
        CreatedAt = u.CreatedAt
    };
}
