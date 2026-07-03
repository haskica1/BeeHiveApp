using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

/// <summary>
/// One educational article (SPEC-06). Platform-wide content: authored by SystemAdmin, visible to all
/// organizations once published. <see cref="Months"/> marks when the topic is seasonal ("aktuelno");
/// null/empty = evergreen.
/// </summary>
public class LearningTopic : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public LearningCategory Category { get; set; }

    /// <summary>Months (1–12) when the topic is current; null = evergreen (Postgres integer[]).</summary>
    public int[]? Months { get; set; }

    /// <summary>Card teaser shown in the list.</summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>The article body, markdown.</summary>
    public string BodyMarkdown { get; set; } = string.Empty;

    public bool IsPublished { get; set; }

    /// <summary>Set on the first publish only — the once-per-topic notification guard.</summary>
    public DateTime? PublishedAt { get; set; }

    public List<LearningTopicRead> Reads { get; set; } = [];
}
