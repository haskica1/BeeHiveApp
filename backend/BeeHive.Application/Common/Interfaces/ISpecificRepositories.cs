using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>Organization-specific data access operations.</summary>
public interface IOrganizationRepository : IRepository<Organization>
{
    Task<IEnumerable<Organization>> GetAllWithDetailsAsync();
    Task<Organization?> GetWithDetailsAsync(int id);
}

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

/// <summary>Diet-specific data access operations.</summary>
public interface IDietRepository : IRepository<Diet>
{
    Task<IEnumerable<Diet>> GetByBeehiveIdAsync(int beehiveId);
    Task<Diet?> GetWithEntriesAsync(int id);
}

/// <summary>FeedingEntry-specific data access operations.</summary>
public interface IFeedingEntryRepository : IRepository<FeedingEntry>
{
    Task<IEnumerable<FeedingEntry>> GetByDietIdAsync(int dietId);
}

/// <summary>Todo-specific data access operations.</summary>
public interface ITodoRepository : IRepository<Todo>
{
    Task<IEnumerable<Todo>> GetByApiaryIdAsync(int apiaryId);
    Task<IEnumerable<Todo>> GetByBeehiveIdAsync(int beehiveId);
    Task<Todo?> GetByIdWithUsersAsync(int id);
}


/// <summary>Apiary-specific data access operations.</summary>
public interface IApiaryRepository : IRepository<Apiary>
{
    /// <summary>Returns the apiary with its beehives eagerly loaded.</summary>
    Task<Apiary?> GetWithBeehivesAsync(int id);

    /// <summary>Returns all apiaries with beehive counts.</summary>
    Task<IEnumerable<Apiary>> GetAllWithBeehivesAsync();

    /// <summary>Returns all apiaries belonging to a specific organization.</summary>
    Task<IEnumerable<Apiary>> GetAllByOrganizationAsync(int organizationId);
}

/// <summary>Beehive-specific data access operations.</summary>
public interface IBeehiveRepository : IRepository<Beehive>
{
    /// <summary>Returns the beehive with its inspections eagerly loaded.</summary>
    Task<Beehive?> GetWithInspectionsAsync(int id);

    /// <summary>Returns all beehives belonging to a specific apiary.</summary>
    Task<IEnumerable<Beehive>> GetByApiaryIdAsync(int apiaryId);

    /// <summary>Returns all beehives belonging to a specific organization (across all its apiaries).</summary>
    Task<IEnumerable<Beehive>> GetByOrganizationAsync(int organizationId);
}

/// <summary>Inspection-specific data access operations.</summary>
public interface IInspectionRepository : IRepository<Inspection>
{
    /// <summary>Returns all inspections for a given beehive, ordered by date descending.</summary>
    Task<IEnumerable<Inspection>> GetByBeehiveIdAsync(int beehiveId);
}
