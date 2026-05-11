namespace BeeHive.Application.Features.Stats.DTOs;

public record StatsDto
{
    public int TotalApiaries { get; init; }
    public int TotalBeehives { get; init; }
    public int TotalInspections { get; init; }
    public int ActiveDiets { get; init; }
    public int PendingTodos { get; init; }

    public IReadOnlyList<NameValueDto> BeehivesByType { get; init; } = [];
    public IReadOnlyList<NameValueDto> BeehivesByMaterial { get; init; } = [];
    public IReadOnlyList<NameValueDto> HoneyLevelDistribution { get; init; } = [];
    public IReadOnlyList<MonthCountDto> InspectionsByMonth { get; init; } = [];
    public IReadOnlyList<MonthTempDto> TemperatureByMonth { get; init; } = [];
    public IReadOnlyList<NameValueDto> DietsByStatus { get; init; } = [];
    public IReadOnlyList<NameValueDto> DietsByFoodType { get; init; } = [];
    public IReadOnlyList<NameValueDto> TopBeehivesByInspections { get; init; } = [];
    public IReadOnlyList<NameValueDto> ApiariesByBeehiveCount { get; init; } = [];
    public IReadOnlyList<PriorityStatsDto> TodosByPriority { get; init; } = [];
}

public record NameValueDto(string Name, int Value);
public record MonthCountDto(string Month, int Count);
public record MonthTempDto(string Month, double? AvgTemp, double? MinTemp, double? MaxTemp);
public record PriorityStatsDto(string Priority, int Total, int Completed);
