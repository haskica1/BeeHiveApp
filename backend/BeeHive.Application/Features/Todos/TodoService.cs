using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Todos.DTOs;
using BeeHive.Domain.Entities;

namespace BeeHive.Application.Features.Todos;

// ── Interface ─────────────────────────────────────────────────────────────────

public interface ITodoService
{
    Task<IEnumerable<TodoDto>> GetByApiaryIdAsync(int apiaryId);
    Task<IEnumerable<TodoDto>> GetByBeehiveIdAsync(int beehiveId);
    Task<TodoDto> GetByIdAsync(int id);
    Task<TodoDto> CreateAsync(CreateTodoDto dto, int? createdById);
    Task<TodoDto> UpdateAsync(int id, UpdateTodoDto dto);
    Task DeleteAsync(int id);
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
        // Validate parent exists
        if (dto.ApiaryId.HasValue && !await _uow.Apiaries.ExistsAsync(dto.ApiaryId.Value))
            throw new NotFoundException(nameof(Apiary), dto.ApiaryId.Value);

        if (dto.BeehiveId.HasValue && !await _uow.Beehives.ExistsAsync(dto.BeehiveId.Value))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId.Value);

        var todo = _mapper.Map<Todo>(dto);
        todo.CreatedById = createdById;

        await _uow.Todos.AddAsync(todo);
        await _uow.SaveChangesAsync();

        return _mapper.Map<TodoDto>(todo);
    }

    public async Task<TodoDto> UpdateAsync(int id, UpdateTodoDto dto)
    {
        var todo = await _uow.Todos.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Todo), id);

        _mapper.Map(dto, todo);

        // Set or clear CompletedAt based on IsCompleted
        if (todo.IsCompleted && todo.CompletedAt is null)
            todo.CompletedAt = DateTime.UtcNow;
        else if (!todo.IsCompleted)
            todo.CompletedAt = null;

        todo.UpdatedAt = DateTime.UtcNow;

        await _uow.Todos.UpdateAsync(todo);
        await _uow.SaveChangesAsync();

        return _mapper.Map<TodoDto>(todo);
    }

    public async Task DeleteAsync(int id)
    {
        var todo = await _uow.Todos.GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(Todo), id);

        await _uow.Todos.DeleteAsync(todo);
        await _uow.SaveChangesAsync();
    }
}
