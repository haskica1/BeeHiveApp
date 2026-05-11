using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Stats.DTOs;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Stats;

public interface IStatsService
{
    Task<StatsDto> GetStatsAsync(int? organizationId);
}

public class StatsService : IStatsService
{
    private readonly IUnitOfWork _uow;

    public StatsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<StatsDto> GetStatsAsync(int? organizationId)
    {
        // ── Fetch base data ────────────────────────────────────────────────────

        var apiaries = organizationId.HasValue
            ? (await _uow.Apiaries.GetAllByOrganizationAsync(organizationId.Value)).ToList()
            : (await _uow.Apiaries.GetAllAsync()).ToList();

        var apiaryIds   = apiaries.Select(a => a.Id).ToHashSet();
        var apiaryNames = apiaries.ToDictionary(a => a.Id, a => a.Name);

        var beehives = organizationId.HasValue
            ? (await _uow.Beehives.GetByOrganizationAsync(organizationId.Value)).ToList()
            : (await _uow.Beehives.GetAllAsync()).ToList();

        var beehiveIds = beehives.Select(b => b.Id).ToHashSet();
        var beehiveNamesById = beehives.ToDictionary(b => b.Id, b => b.Name);

        var inspections = beehiveIds.Count > 0
            ? (await _uow.Inspections.FindAsync(i => beehiveIds.Contains(i.BeehiveId))).ToList()
            : [];

        var diets = beehiveIds.Count > 0
            ? (await _uow.Diets.FindAsync(d => beehiveIds.Contains(d.BeehiveId))).ToList()
            : [];

        var todos = beehiveIds.Count > 0 || apiaryIds.Count > 0
            ? (await _uow.Todos.FindAsync(t =>
                (t.ApiaryId.HasValue  && apiaryIds.Contains(t.ApiaryId.Value)) ||
                (t.BeehiveId.HasValue && beehiveIds.Contains(t.BeehiveId.Value))
              )).ToList()
            : [];

        // ── Summary ────────────────────────────────────────────────────────────

        var activeDiets   = diets.Count(d => d.Status == DietStatus.InProgress || d.Status == DietStatus.NotStarted);
        var pendingTodos  = todos.Count(t => !t.IsCompleted);

        // ── Beehive distributions ──────────────────────────────────────────────

        var byType = beehives
            .GroupBy(b => b.Type)
            .Select(g => new NameValueDto(FormatEnum(g.Key.ToString()), g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        var byMaterial = beehives
            .GroupBy(b => b.Material)
            .Select(g => new NameValueDto(FormatEnum(g.Key.ToString()), g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        // ── Honey level distribution ───────────────────────────────────────────

        var honeyDist = inspections
            .GroupBy(i => i.HoneyLevel)
            .Select(g => new NameValueDto(FormatEnum(g.Key.ToString()), g.Count()))
            .OrderBy(x => x.Name)
            .ToList();

        // ── Inspections per month (last 12 months) ─────────────────────────────

        var last12 = GenerateLast12Months();
        var inspByMonth = inspections
            .Where(i => i.Date >= DateTime.UtcNow.AddMonths(-12))
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .ToDictionary(g => (g.Key.Year, g.Key.Month), g => g.Count());

        var inspectionsByMonth = last12
            .Select(m => new MonthCountDto(
                m.Label,
                inspByMonth.TryGetValue((m.Year, m.Month), out var c) ? c : 0))
            .ToList();

        // ── Temperature by month (last 12 months) ──────────────────────────────

        var tempByMonth = inspections
            .Where(i => i.Date >= DateTime.UtcNow.AddMonths(-12) && i.Temperature.HasValue)
            .GroupBy(i => new { i.Date.Year, i.Date.Month })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => (
                    Avg: Math.Round(g.Average(i => i.Temperature!.Value), 1),
                    Min: Math.Round(g.Min(i => i.Temperature!.Value), 1),
                    Max: Math.Round(g.Max(i => i.Temperature!.Value), 1)
                )
            );

        var temperatureByMonth = last12
            .Select(m => tempByMonth.TryGetValue((m.Year, m.Month), out var t)
                ? new MonthTempDto(m.Label, t.Avg, t.Min, t.Max)
                : new MonthTempDto(m.Label, null, null, null))
            .ToList();

        // ── Diet distributions ─────────────────────────────────────────────────

        var dietsByStatus = diets
            .GroupBy(d => d.Status)
            .Select(g => new NameValueDto(FormatEnum(g.Key.ToString()), g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        var dietsByFoodType = diets
            .GroupBy(d => d.FoodType)
            .Select(g => new NameValueDto(
                g.Key == FoodType.Custom
                    ? "Custom"
                    : FormatEnum(g.Key.ToString()),
                g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        // ── Top beehives by inspection count ──────────────────────────────────

        var topBeehives = beehives
            .Select(b => new NameValueDto(
                b.Name,
                inspections.Count(i => i.BeehiveId == b.Id)))
            .OrderByDescending(x => x.Value)
            .Take(8)
            .ToList();

        // ── Apiaries by beehive count ──────────────────────────────────────────

        var apiariesByCount = beehives
            .GroupBy(b => b.ApiaryId)
            .Select(g => new NameValueDto(
                apiaryNames.TryGetValue(g.Key, out var name) ? name : $"Apiary {g.Key}",
                g.Count()))
            .OrderByDescending(x => x.Value)
            .ToList();

        // ── Todos by priority ──────────────────────────────────────────────────

        var todosByPriority = todos
            .GroupBy(t => t.Priority)
            .Select(g => new PriorityStatsDto(
                FormatEnum(g.Key.ToString()),
                g.Count(),
                g.Count(t => t.IsCompleted)))
            .OrderByDescending(x => x.Total)
            .ToList();

        // ── Build result ───────────────────────────────────────────────────────

        return new StatsDto
        {
            TotalApiaries         = apiaries.Count,
            TotalBeehives         = beehives.Count,
            TotalInspections      = inspections.Count,
            ActiveDiets           = activeDiets,
            PendingTodos          = pendingTodos,
            BeehivesByType        = byType,
            BeehivesByMaterial    = byMaterial,
            HoneyLevelDistribution= honeyDist,
            InspectionsByMonth    = inspectionsByMonth,
            TemperatureByMonth    = temperatureByMonth,
            DietsByStatus         = dietsByStatus,
            DietsByFoodType       = dietsByFoodType,
            TopBeehivesByInspections = topBeehives,
            ApiariesByBeehiveCount   = apiariesByCount,
            TodosByPriority          = todosByPriority,
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static string FormatEnum(string raw)
    {
        // "DadantBlatt" → "Dadant Blatt", "InProgress" → "In Progress"
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < raw.Length; i++)
        {
            if (i > 0 && char.IsUpper(raw[i]))
                result.Append(' ');
            result.Append(raw[i]);
        }
        return result.ToString();
    }

    private static IReadOnlyList<(int Year, int Month, string Label)> GenerateLast12Months()
    {
        var list = new List<(int, int, string)>();
        var now = DateTime.UtcNow;
        for (int i = 11; i >= 0; i--)
        {
            var d = now.AddMonths(-i);
            list.Add((d.Year, d.Month, d.ToString("MMM yy")));
        }
        return list;
    }
}
