using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>FeedingEntry-specific data access operations.</summary>
public interface IFeedingEntryRepository : IRepository<FeedingEntry>
{
    Task<IEnumerable<FeedingEntry>> GetByDietIdAsync(int dietId);
}
