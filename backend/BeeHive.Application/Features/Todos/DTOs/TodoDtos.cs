using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Todos.DTOs;

/// <summary>Read model for a to-do item.</summary>
public class TodoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ApiaryId { get; set; }
    public int? BeehiveId { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    /// <summary>Set when creating a todo for an apiary.</summary>
    public int? ApiaryId { get; set; }

    /// <summary>Set when creating a todo for a beehive.</summary>
    public int? BeehiveId { get; set; }
}

public class UpdateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
}
