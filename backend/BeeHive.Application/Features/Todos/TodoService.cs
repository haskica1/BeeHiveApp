using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Todos.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Todos;

// ── Assignable user DTO ───────────────────────────────────────────────────────

public record AssignableUserDto(int Id, string FullName);

// ── Interface ─────────────────────────────────────────────────────────────────

public interface ITodoService
{
    Task<IEnumerable<TodoDto>> GetByApiaryIdAsync(int apiaryId);
    Task<IEnumerable<TodoDto>> GetByBeehiveIdAsync(int beehiveId);
    Task<TodoDto> GetByIdAsync(int id);
    Task<TodoDto> CreateAsync(CreateTodoDto dto, int? createdById);
    Task<TodoDto> UpdateAsync(int id, UpdateTodoDto dto);
    Task DeleteAsync(int id);
    Task<IEnumerable<AssignableUserDto>> GetAssignableUsersAsync(string callerRole, int? callerUserId, int? callerOrgId, int? callerApiaryId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class TodoService : ITodoService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public TodoService(IUnitOfWork uow, IMapper mapper)
    {
        _uow    = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TodoDto>> GetByApiaryIdAsync(int apiaryId)
    {
        if (!await _uow.Apiaries.ExistsAsync(apiaryId))
            throw new NotFoundException(nameof(Apiary), apiaryId);

        var todos = await _uow.Todos.GetByApiaryIdAsync(apiaryId);
        return _mapper.Map<IEnumerable<TodoDto>>(todos);
    }

    public async Task<IEnumerable<TodoDto>> GetByBeehiveIdAsync(int beehiveId)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        var todos = await _uow.Todos.GetByBeehiveIdAsync(beehiveId);
        return _mapper.Map<IEnumerable<TodoDto>>(todos);
    }

    public async Task<TodoDto> GetByIdAsync(int id)
    {
        var todo = await _uow.Todos.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Todo), id);

        return _mapper.Map<TodoDto>(todo);
    }

    public async Task<TodoDto> CreateAsync(CreateTodoDto dto, int? createdById)
    {
        if (dto.ApiaryId.HasValue && !await _uow.Apiaries.ExistsAsync(dto.ApiaryId.Value))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);

        if (dto.BeehiveId.HasValue && !await _uow.Beehives.ExistsAsync(dto.BeehiveId.Value))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId.Value);

        var todo = _mapper.Map<Todo>(dto);
        todo.CreatedById = createdById;

        await _uow.Todos.AddAsync(todo);
        await _uow.SaveChangesAsync();

        // Reload with navigation properties for the response
        var created = await _uow.Todos.GetByIdWithUsersAsync(todo.Id);
        return _mapper.Map<TodoDto>(created!);
    }

    public async Task<TodoDto> UpdateAsync(int id, UpdateTodoDto dto)
    {
        var todo = await _uow.Todos.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Todo), id);

        _mapper.Map(dto, todo);

        if (todo.IsCompleted && todo.CompletedAt is null)
            todo.CompletedAt = DateTime.UtcNow;
        else if (!todo.IsCompleted)
            todo.CompletedAt = null;

        todo.UpdatedAt = DateTime.UtcNow;

        await _uow.Todos.UpdateAsync(todo);
        await _uow.SaveChangesAsync();

        var updated = await _uow.Todos.GetByIdWithUsersAsync(id);
        return _mapper.Map<TodoDto>(updated!);
    }

    public async Task<IEnumerable<AssignableUserDto>> GetAssignableUsersAsync(
        string callerRole, int? callerUserId, int? callerOrgId, int? callerApiaryId)
    {
        IEnumerable<User> users;

        if (callerRole == nameof(UserRole.SystemAdmin) || callerRole == nameof(UserRole.OrgAdmin))
        {
            // Org-level: all users in the same organization
            users = callerOrgId.HasValue
                ? await _uow.Users.FindAsync(u => u.OrganizationId == callerOrgId)
                : Enumerable.Empty<User>();
        }
        else if (callerRole == nameof(UserRole.Admin))
        {
            // Apiary-level: all users assigned to the same apiary
            users = callerApiaryId.HasValue
                ? await _uow.Users.FindAsync(u => u.ApiaryId == callerApiaryId)
                : Enumerable.Empty<User>();
        }
        else
        {
            // Regular user: only themselves
            users = callerUserId.HasValue
                ? await _uow.Users.FindAsync(u => u.Id == callerUserId)
                : Enumerable.Empty<User>();
        }

        return users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new AssignableUserDto(u.Id, $"{u.FirstName} {u.LastName}"));
    }

    public async Task DeleteAsync(int id)
    {
        var todo = await _uow.Todos.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Todo), id);

        await _uow.Todos.DeleteAsync(todo);
        await _uow.SaveChangesAsync();
    }
}
