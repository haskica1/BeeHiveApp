namespace BeeHive.Application.Common.Interfaces;

/// <summary>
/// Unit of Work pattern — coordinates repositories and persists changes atomically.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IOrganizationRepository Organizations { get; }
    IUserRepository Users { get; }
    IApiaryRepository Apiaries { get; }
    IBeehiveRepository Beehives { get; }
    IInspectionRepository Inspections { get; }
    IQueenRepository Queens { get; }
    ITodoRepository Todos { get; }
    IDietRepository Diets { get; }
    IFeedingEntryRepository FeedingEntries { get; }
    IExpenseRepository Expenses { get; }
    IHarvestRepository Harvests { get; }
    INotificationRepository Notifications { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IAdvisorConversationRepository AdvisorConversations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
