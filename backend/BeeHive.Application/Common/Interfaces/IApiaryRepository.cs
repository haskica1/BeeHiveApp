using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

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
