using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.Apiaries.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Apiaries;

public class ApiaryService : IApiaryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly IAccessGuard _access;

    public ApiaryService(IUnitOfWork uow, IMapper mapper, ICurrentUser currentUser, IAccessGuard access)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _access = access;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ApiaryDto>> GetAllForCurrentUserAsync()
    {
        if (_currentUser.OrganizationId is not int organizationId)
            return [];

        var apiaries = await _uow.Apiaries.GetAllByOrganizationAsync(organizationId);

        switch (_currentUser.Role)
        {
            // An ApiaryAdmin only sees their assigned apiary.
            case UserRole.Admin:
                apiaries = _currentUser.ApiaryId is int apiaryId
                    ? apiaries.Where(a => a.Id == apiaryId)
                    : [];
                break;

            // A Beekeeper only sees apiaries that contain a beehive assigned to them.
            case UserRole.User:
                var assignedApiaryIds = await _access.GetAssignedApiaryIdsAsync();
                apiaries = apiaries.Where(a => assignedApiaryIds.Contains(a.Id));
                break;
        }

        return _mapper.Map<IEnumerable<ApiaryDto>>(apiaries);
    }

    /// <inheritdoc />
    public async Task<ApiaryDetailDto> GetByIdAsync(int id)
    {
        var apiary = await _uow.Apiaries.GetWithBeehivesAsync(id)
            ?? throw new NotFoundException(nameof(Apiary), id);

        // A Beekeeper may view an apiary only through the beehives assigned to them.
        if (_currentUser.Role == UserRole.User)
        {
            var assignedIds = await _access.GetAssignedBeehiveIdsAsync();
            var dto = _mapper.Map<ApiaryDetailDto>(apiary);
            var visible = dto.Beehives.Where(b => assignedIds.Contains(b.Id)).ToList();
            if (visible.Count == 0)
                throw new ForbiddenAccessException();

            dto.Beehives = visible;
            dto.BeehiveCount = visible.Count;
            return dto;
        }

        // Managers must own the apiary (same org / same apiary).
        _access.EnsureCanManageApiary(apiary.Id, apiary.OrganizationId);
        return _mapper.Map<ApiaryDetailDto>(apiary);
    }

    /// <inheritdoc />
    public async Task<ApiaryDto> CreateAsync(CreateApiaryDto dto)
    {
        if (_currentUser.OrganizationId is not int organizationId)
            throw new ForbiddenAccessException("You must belong to an organization to create an apiary.");

        var apiary = _mapper.Map<Apiary>(dto);
        apiary.OrganizationId = organizationId;
        apiary.CreatedById = _currentUser.UserId;
        await _uow.Apiaries.AddAsync(apiary);
        await _uow.SaveChangesAsync();
        // Reload to get CreatedBy nav property
        var saved = await _uow.Apiaries.GetWithBeehivesAsync(apiary.Id) ?? apiary;
        return _mapper.Map<ApiaryDto>(saved);
    }

    /// <inheritdoc />
    public async Task<ApiaryDto> UpdateAsync(int id, UpdateApiaryDto dto)
    {
        var apiary = await _uow.Apiaries.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Apiary), id);

        _access.EnsureCanManageApiary(apiary.Id, apiary.OrganizationId);

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

        _access.EnsureCanManageApiary(apiary.Id, apiary.OrganizationId);

        // Apiary → Todos has NO ACTION cascade (to avoid multiple-cascade-path errors).
        // Delete apiary-level todos explicitly before removing the apiary.
        // Beehive-level todos are handled by the existing Beehive → Todos cascade.
        var apiaryTodos = await _uow.Todos.GetByApiaryIdAsync(id);
        foreach (var todo in apiaryTodos)
            await _uow.Todos.DeleteAsync(todo);

        await _uow.Apiaries.DeleteAsync(apiary);
        await _uow.SaveChangesAsync();
    }
}
