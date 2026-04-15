using BeeHive.Application.Common.Interfaces;
using BeeHive.Infrastructure.Data.Repositories;

namespace BeeHive.Infrastructure.Data;

/// <summary>
/// EF Core Unit of Work implementation.
/// Wraps all repositories and exposes a single SaveChangesAsync entry point,
/// ensuring all changes within a request are committed atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BeeHiveDbContext _context;

    // Lazy-initialised repositories — only created when first accessed
    private IApiaryRepository? _apiaries;
    private IBeehiveRepository? _beehives;
    private IInspectionRepository? _inspections;
    private ITodoRepository? _todos;

    public UnitOfWork(BeeHiveDbContext context)
    {
        _context = context;
    }

    public IApiaryRepository Apiaries =>
        _apiaries ??= new ApiaryRepository(_context);

    public IBeehiveRepository Beehives =>
        _beehives ??= new BeehiveRepository(_context);

    public IInspectionRepository Inspections =>
        _inspections ??= new InspectionRepository(_context);

    public ITodoRepository Todos =>
        _todos ??= new TodoRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
