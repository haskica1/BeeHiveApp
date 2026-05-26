using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Notifications;
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
    Task<IEnumerable<AssignableUserDto>> GetAssignableUsersForBeehiveAsync(int beehiveId, string callerRole, int? callerUserId, int? callerOrgId, int? callerApiaryId);
    Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class TodoService : ITodoService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationService _notifications;

    public TodoService(IUnitOfWork uow, IMapper mapper, INotificationService notifications)
    {
        _uow           = uow;
        _mapper        = mapper;
        _notifications = notifications;
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

        var created = await _uow.Todos.GetByIdWithUsersAsync(todo.Id);

        // 6) Todo creation notifications
        if (createdById.HasValue)
        {
            var creator = await _uow.Users.GetByIdWithOrganizationAsync(createdById.Value);
            if (creator != null)
                await SendTodoCreatedNotificationsAsync(created!, creator);
        }

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
            users = callerOrgId.HasValue
                ? await _uow.Users.FindAsync(u => u.OrganizationId == callerOrgId)
                : Enumerable.Empty<User>();
        }
        else if (callerRole == nameof(UserRole.Admin))
        {
            users = callerApiaryId.HasValue
                ? await _uow.Users.FindAsync(u => u.ApiaryId == callerApiaryId)
                : Enumerable.Empty<User>();
        }
        else
        {
            users = callerUserId.HasValue
                ? await _uow.Users.FindAsync(u => u.Id == callerUserId)
                : Enumerable.Empty<User>();
        }

        return users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new AssignableUserDto(u.Id, $"{u.FirstName} {u.LastName}"));
    }

    public async Task<IEnumerable<AssignableUserDto>> GetAssignableUsersForBeehiveAsync(
        int beehiveId, string callerRole, int? callerUserId, int? callerOrgId, int? callerApiaryId)
    {
        var beehive = await _uow.Beehives.GetByIdAsync(beehiveId);
        if (beehive == null)
            throw new NotFoundException(nameof(Beehive), beehiveId);

        var results = new HashSet<User>();

        var admins = await _uow.Users.FindAsync(u =>
            u.ApiaryId == beehive.ApiaryId && u.Role == UserRole.Admin);
        foreach (var admin in admins)
            results.Add(admin);

        var beehiveUsers = await _uow.Users.FindAsync(u =>
            u.AssignedBeehives.Any(ub => ub.BeehiveId == beehiveId));
        foreach (var user in beehiveUsers)
            results.Add(user);

        return results
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

    public Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId) =>
        _uow.Users.IsUserAssignedToBeehiveAsync(userId, beehiveId);

    // ── Notification helpers ──────────────────────────────────────────────────

    private async Task SendTodoCreatedNotificationsAsync(Todo todo, User creator)
    {
        // Resolve the apiary for context label
        int? apiaryId = todo.ApiaryId;
        if (!apiaryId.HasValue && todo.BeehiveId.HasValue)
        {
            var beehive = await _uow.Beehives.GetByIdAsync(todo.BeehiveId.Value);
            apiaryId = beehive?.ApiaryId;
        }

        var apiary = apiaryId.HasValue ? await _uow.Apiaries.GetByIdAsync(apiaryId.Value) : null;
        var context = apiary != null ? $" in apiary '{apiary.Name}'" : string.Empty;

        // Notify creator's superior (same cascading rule as beehive creation)
        if (creator.Role == UserRole.Admin)
        {
            var orgAdmins = await _uow.Users.FindAsync(u =>
                u.OrganizationId == creator.OrganizationId && u.Role == UserRole.OrgAdmin);

            foreach (var orgAdmin in orgAdmins)
            {
                if (orgAdmin.Id == creator.Id) continue;
                await _notifications.NotifyAsync(
                    orgAdmin.Id,
                    "New TODO created",
                    $"Admin {creator.FirstName} {creator.LastName} created todo '{todo.Title}'{context}.",
                    NotificationType.TodoCreated,
                    todo.Id, nameof(Todo));
            }
        }
        else if (creator.Role == UserRole.OrgAdmin && apiaryId.HasValue)
        {
            var admins = await _uow.Users.FindAsync(u =>
                u.ApiaryId == apiaryId.Value && u.Role == UserRole.Admin);

            foreach (var admin in admins)
            {
                if (admin.Id == creator.Id) continue;
                await _notifications.NotifyAsync(
                    admin.Id,
                    "New TODO created",
                    $"Organization Admin {creator.FirstName} {creator.LastName} created todo '{todo.Title}'{context}.",
                    NotificationType.TodoCreated,
                    todo.Id, nameof(Todo));
            }
        }

        // Notify assignee if different from creator
        if (todo.AssignedToId.HasValue && todo.AssignedToId != creator.Id)
        {
            await _notifications.NotifyAsync(
                todo.AssignedToId.Value,
                "TODO assigned to you",
                $"'{todo.Title}' has been assigned to you{context}.",
                NotificationType.TodoCreated,
                todo.Id, nameof(Todo));
        }
    }
}
