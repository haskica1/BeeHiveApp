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

    /// <summary>IDs of the beehives assigned to the user — no entities loaded.</summary>
    Task<HashSet<int>> GetAssignedBeehiveIdsAsync(int userId);

    /// <summary>IDs of the apiaries containing the user's assigned beehives — no entities loaded.</summary>
    Task<HashSet<int>> GetAssignedApiaryIdsAsync(int userId);
}
