using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Diets.DTOs;

// ── Read DTOs ────────────────────────────────────────────────────────────────

public class DietDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DietReason Reason { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string? CustomReason { get; set; }
    public int DurationDays { get; set; }
    public int FrequencyDays { get; set; }
    public FoodType FoodType { get; set; }
    public string FoodTypeName { get; set; } = string.Empty;
    public string? CustomFoodType { get; set; }
    public DietStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? EarlyCompletionComment { get; set; }
    public int BeehiveId { get; set; }
    public int TotalEntries { get; set; }
    public int CompletedEntries { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DietDetailDto : DietDto
{
    public List<FeedingEntryDto> FeedingEntries { get; set; } = new();
}

public class FeedingEntryDto
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public FeedingEntryStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime? CompletionDate { get; set; }
    public int DietId { get; set; }
}

// ── Write DTOs ───────────────────────────────────────────────────────────────

public class CreateDietDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DietReason Reason { get; set; }
    public string? CustomReason { get; set; }
    public int DurationDays { get; set; }
    public int FrequencyDays { get; set; }
    public FoodType FoodType { get; set; }
    public string? CustomFoodType { get; set; }
    public int BeehiveId { get; set; }
}

public class UpdateDietDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DietReason Reason { get; set; }
    public string? CustomReason { get; set; }
    public int DurationDays { get; set; }
    public int FrequencyDays { get; set; }
    public FoodType FoodType { get; set; }
    public string? CustomFoodType { get; set; }
}

public class CompleteEarlyDto
{
    public string Comment { get; set; } = string.Empty;
}
