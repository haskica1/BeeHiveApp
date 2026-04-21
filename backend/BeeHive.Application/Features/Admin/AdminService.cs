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
    Task<AdminOrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto);
    Task<AdminOrganizationDto> UpdateOrganizationAsync(int id, UpdateOrganizationDto dto);
    Task DeleteOrganizationAsync(int id);

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

    public async Task<AdminOrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto)
    {
        var org = new Organization
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Organizations.AddAsync(org);
        await _uow.SaveChangesAsync();

        return MapOrganization(org);
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

    // ── Users ──────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _uow.Users.GetAllWithOrganizationAsync();
        return users.Select(MapUser);
    }

    public async Task<AdminUserDto> GetUserByIdAsync(int id)
    {
        var user = await _uow.Users.GetByIdWithOrganizationAsync(id)
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

        if (role != UserRole.SystemAdmin && dto.OrganizationId == null)
            throw new BusinessRuleException("Non-SystemAdmin users must belong to an organization.");

        if (dto.OrganizationId.HasValue)
        {
            var orgExists = await _uow.Organizations.ExistsAsync(dto.OrganizationId.Value);
            if (!orgExists)
                throw new NotFoundException(nameof(Organization), dto.OrganizationId.Value);
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            OrganizationId = dto.OrganizationId,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.Users.AddAsync(user);
        await _uow.SaveChangesAsync();

        // reload with organization
        var created = await _uow.Users.GetByEmailAsync(user.Email);
        return MapUser(created!);
    }

    public async Task<AdminUserDto> UpdateUserAsync(int id, UpdateAdminUserDto dto)
    {
        var user = await _uow.Users.GetByIdWithOrganizationAsync(id)
            ?? throw new NotFoundException(nameof(User), id);

        if (!Enum.TryParse<UserRole>(dto.Role, out var role))
            throw new BusinessRuleException($"Invalid role '{dto.Role}'.");

        if (role != UserRole.SystemAdmin && dto.OrganizationId == null)
            throw new BusinessRuleException("Non-SystemAdmin users must belong to an organization.");

        if (dto.OrganizationId.HasValue)
        {
            var orgExists = await _uow.Organizations.ExistsAsync(dto.OrganizationId.Value);
            if (!orgExists)
                throw new NotFoundException(nameof(Organization), dto.OrganizationId.Value);
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

        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Users.GetByIdWithOrganizationAsync(id);
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

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static AdminOrganizationDto MapOrganization(Organization o) => new()
    {
        Id = o.Id,
        Name = o.Name,
        Description = o.Description,
        UserCount = o.Users.Count,
        ApiaryCount = o.Apiaries.Count,
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
        CreatedAt = u.CreatedAt
    };
}
