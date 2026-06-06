using BeeHive.Application.Common.Interfaces;
using BeeHive.Entity.Repositories;

namespace BeeHive.Entity;

/// <summary>
/// EF Core Unit of Work implementation.
/// Wraps all repositories and exposes a single SaveChangesAsync entry point,
/// ensuring all changes within a request are committed atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly BeeHiveDbContext _context;

    // Lazy-initialised repositories — only created when first accessed
    private IOrganizationRepository? _organizations;
    private IUserRepository? _users;
    private IApiaryRepository? _apiaries;
    private IBeehiveRepository? _beehives;
    private IInspectionRepository? _inspections;
    private ITodoRepository? _todos;
    private IDietRepository? _diets;
    private IFeedingEntryRepository? _feedingEntries;
    private IExpenseRepository? _expenses;
    private INotificationRepository? _notifications;

    public UnitOfWork(BeeHiveDbContext context)
    {
        _context = context;
    }

    public IOrganizationRepository Organizations =>
        _organizations ??= new OrganizationRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IApiaryRepository Apiaries =>
        _apiaries ??= new ApiaryRepository(_context);

    public IBeehiveRepository Beehives =>
        _beehives ??= new BeehiveRepository(_context);

    public IInspectionRepository Inspections =>
        _inspections ??= new InspectionRepository(_context);

    public ITodoRepository Todos =>
        _todos ??= new TodoRepository(_context);

    public IDietRepository Diets =>
        _diets ??= new DietRepository(_context);

    public IFeedingEntryRepository FeedingEntries =>
        _feedingEntries ??= new FeedingEntryRepository(_context);

    public IExpenseRepository Expenses =>
        _expenses ??= new ExpenseRepository(_context);

    public INotificationRepository Notifications =>
        _notifications ??= new NotificationRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}
