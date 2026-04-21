namespace BeeHive.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern — coordinates repositories and persists changes atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IApiaryRepository Apiaries { get; }
    IBeehiveRepository Beehives { get; }
    IInspectionRepository Inspections { get; }
    ITodoRepository Todos { get; }
    IDietRepository Diets { get; }
    IFeedingEntryRepository FeedingEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
