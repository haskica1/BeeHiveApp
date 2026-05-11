namespace BeeHive.Application.Features.Calendar.DTOs;

public class CalendarEventsDto
{
    public List<CalendarTodoDto> Todos { get; set; } = new();
    public List<CalendarFeedingEntryDto> FeedingEntries { get; set; } = new();
}

public class CalendarTodoDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int? ApiaryId { get; set; }
    public string? ApiaryName { get; set; }
    public int? BeehiveId { get; set; }
    public string? BeehiveName { get; set; }
}

public class CalendarFeedingEntryDto
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int DietId { get; set; }
    public string DietName { get; set; } = string.Empty;
    public int BeehiveId { get; set; }
    public string BeehiveName { get; set; } = string.Empty;
    public string FoodTypeName { get; set; } = string.Empty;
}
