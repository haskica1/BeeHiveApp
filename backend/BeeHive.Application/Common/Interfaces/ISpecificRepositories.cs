using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>Apiary-specific data access operations.</summary>
public interface IApiaryRepository : IRepository<Apiary>
{
    /// <summary>Returns the apiary with its beehives eagerly loaded.</summary>
    Task<Apiary?> GetWithBeehivesAsync(int id);

    /// <summary>Returns all apiaries with beehive counts.</summary>
    Task<IEnumerable<Apiary>> GetAllWithBeehivesAsync();
}

/// <summary>Beehive-specific data access operations.</summary>
public interface IBeehiveRepository : IRepository<Beehive>
{
    /// <summary>Returns the beehive with its inspections eagerly loaded.</summary>
    Task<Beehive?> GetWithInspectionsAsync(int id);

    /// <summary>Returns all beehives belonging to a specific apiary.</summary>
    Task<IEnumerable<Beehive>> GetByApiaryIdAsync(int apiaryId);
}

/// <summary>Inspection-specific data access operations.</summary>
public interface IInspectionRepository : IRepository<Inspection>
{
    /// <summary>Returns all inspections for a given beehive, ordered by date descending.</summary>
    Task<IEnumerable<Inspection>> GetByBeehiveIdAsync(int beehiveId);
}
