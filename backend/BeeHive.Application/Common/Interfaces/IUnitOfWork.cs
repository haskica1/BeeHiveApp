namespace BeeHive.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern — coordinates repositories and persists changes atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IApiaryRepository Apiaries { get; }
    IBeehiveRepository Beehives { get; }
    IInspectionRepository Inspections { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
