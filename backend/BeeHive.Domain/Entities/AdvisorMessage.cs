using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

/// <summary>A single message within an <see cref="AdvisorConversation"/> (user prompt or assistant reply).</summary>
public class AdvisorMessage : BaseEntity
{
    public int ConversationId { get; set; }
    public AdvisorConversation Conversation { get; set; } = null!;

    public AdvisorRole Role { get; set; }

    public string Content { get; set; } = string.Empty;
}
