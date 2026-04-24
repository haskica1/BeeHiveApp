using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

/// <summary>
/// Represents a task (to-do item) associated with either an apiary or a beehive.
/// Exactly one of ApiaryId or BeehiveId must be set.
/// </summary>
public class Todo : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTime? DueDate { get; set; }

    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    // Foreign keys — exactly one must be non-null
    public int? ApiaryId  { get; set; }
    public int? BeehiveId { get; set; }

    // Navigation properties
    public Apiary?  Apiary  { get; set; }
    public Beehive? Beehive { get; set; }
}
