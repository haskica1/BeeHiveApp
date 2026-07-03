using BeeHive.Domain.Entities;

namespace BeeHive.Application.Common.Interfaces;

/// <summary>Advisor conversation data access.</summary>
public interface IAdvisorConversationRepository : IRepository<AdvisorConversation>
{
    /// <summary>A user's conversations (with Beehive for the name), newest activity first, without message rows.</summary>
    Task<IEnumerable<AdvisorConversation>> GetByUserAsync(int userId);

    /// <summary>A single conversation with its messages (ordered) and beehive — tracked for appends.</summary>
    Task<AdvisorConversation?> GetWithMessagesAsync(int id);
}
