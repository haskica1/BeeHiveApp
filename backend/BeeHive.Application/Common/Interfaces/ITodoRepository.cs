using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>Todo-specific data access operations.</summary>
public interface ITodoRepository : IRepository<Todo>
{
    Task<IEnumerable<Todo>> GetByApiaryIdAsync(int apiaryId);
    Task<IEnumerable<Todo>> GetByBeehiveIdAsync(int beehiveId);
    Task<Todo?> GetByIdWithUsersAsync(int id);
}
