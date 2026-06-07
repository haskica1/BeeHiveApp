using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Todos.DTOs;

public class UpdateTodoDto
{
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public TodoPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
    public int? AssignedToId { get; set; }
}
