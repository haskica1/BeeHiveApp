using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>User-specific data access operations.</summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllWithOrganizationAsync();
    Task<User?> GetByIdWithOrganizationAsync(int id);
    Task<User?> GetByIdWithAssignedBeehivesAsync(int id);
    Task<bool> IsUserAssignedToBeehiveAsync(int userId, int beehiveId);
    Task SetBeehiveAssignmentsAsync(int userId, IEnumerable<int> beehiveIds);
}
