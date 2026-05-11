using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Calendar.DTOs;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Calendar;

public interface ICalendarService
{
    Task<CalendarEventsDto> GetCalendarEventsAsync(string role, int? userId, int? orgId, int? apiaryId);
}

public class CalendarService : ICalendarService
{
    private readonly IUnitOfWork _uow;

    public CalendarService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CalendarEventsDto> GetCalendarEventsAsync(
        string role, int? userId, int? orgId, int? apiaryId)
    {
        // ── Step 1: Resolve accessible IDs and name lookup dictionaries ───────────

        HashSet<int> accessibleApiaryIds;
        HashSet<int> accessibleBeehiveIds;
        Dictionary<int, string> beehiveNames;
        Dictionary<int, string> apiaryNames;

        if (role == "SystemAdmin")
        {
            var allBeehives = (await _uow.Beehives.GetAllAsync()).ToList();
            var allApiaries = (await _uow.Apiaries.GetAllAsync()).ToList();
            accessibleBeehiveIds = allBeehives.Select(b => b.Id).ToHashSet();
            accessibleApiaryIds  = allApiaries.Select(a => a.Id).ToHashSet();
            beehiveNames = allBeehives.ToDictionary(b => b.Id, b => b.Name);
            apiaryNames  = allApiaries.ToDictionary(a => a.Id, a => a.Name);
        }
        else if (role == "OrgAdmin" && orgId.HasValue)
        {
            var beehives = (await _uow.Beehives.GetByOrganizationAsync(orgId.Value)).ToList();
            var apiaries = (await _uow.Apiaries.GetAllByOrganizationAsync(orgId.Value)).ToList();
            accessibleBeehiveIds = beehives.Select(b => b.Id).ToHashSet();
            accessibleApiaryIds  = apiaries.Select(a => a.Id).ToHashSet();
            beehiveNames = beehives.ToDictionary(b => b.Id, b => b.Name);
            apiaryNames  = apiaries.ToDictionary(a => a.Id, a => a.Name);
        }
        else if (role == "Admin" && apiaryId.HasValue)
        {
            var beehives = (await _uow.Beehives.GetByApiaryIdAsync(apiaryId.Value)).ToList();
            var apiary   = await _uow.Apiaries.GetByIdAsync(apiaryId.Value);
            accessibleBeehiveIds = beehives.Select(b => b.Id).ToHashSet();
            accessibleApiaryIds  = new HashSet<int> { apiaryId.Value };
            beehiveNames = beehives.ToDictionary(b => b.Id, b => b.Name);
            apiaryNames  = apiary != null
                ? new Dictionary<int, string> { { apiary.Id, apiary.Name } }
                : new Dictionary<int, string>();
        }
        else if (role == "User" && userId.HasValue)
        {
            var user        = await _uow.Users.GetByIdWithAssignedBeehivesAsync(userId.Value);
            var assignedIds = user?.AssignedBeehives?.Select(ub => ub.BeehiveId).ToHashSet()
                              ?? new HashSet<int>();

            var beehives = assignedIds.Count > 0
                ? (await _uow.Beehives.FindAsync(b => assignedIds.Contains(b.Id))).ToList()
                : new List<BeeHive.Domain.Entities.Beehive>();

            accessibleBeehiveIds = assignedIds;
            accessibleApiaryIds  = beehives.Select(b => b.ApiaryId).ToHashSet();
            beehiveNames         = beehives.ToDictionary(b => b.Id, b => b.Name);

            var apIds    = accessibleApiaryIds;
            var apiaries = apIds.Count > 0
                ? (await _uow.Apiaries.FindAsync(a => apIds.Contains(a.Id))).ToList()
                : new List<BeeHive.Domain.Entities.Apiary>();
            apiaryNames = apiaries.ToDictionary(a => a.Id, a => a.Name);
        }
        else
        {
            return new CalendarEventsDto();
        }

        // ── Step 2: Load todos ────────────────────────────────────────────────────

        List<BeeHive.Domain.Entities.Todo> todos;

        if (role == "User" && userId.HasValue)
        {
            todos = (await _uow.Todos.FindAsync(t => t.AssignedToId == userId.Value)).ToList();
        }
        else if (accessibleApiaryIds.Count > 0 || accessibleBeehiveIds.Count > 0)
        {
            todos = (await _uow.Todos.FindAsync(t =>
                (t.ApiaryId.HasValue  && accessibleApiaryIds.Contains(t.ApiaryId.Value)) ||
                (t.BeehiveId.HasValue && accessibleBeehiveIds.Contains(t.BeehiveId.Value))
            )).ToList();
        }
        else
        {
            todos = new List<BeeHive.Domain.Entities.Todo>();
        }

        // ── Step 3: Load diets with feeding entries ───────────────────────────────

        var diets = accessibleBeehiveIds.Count > 0
            ? (await _uow.Diets.GetByBeehiveIdsAsync(accessibleBeehiveIds)).ToList()
            : new List<BeeHive.Domain.Entities.Diet>();

        // ── Step 4: Build response ────────────────────────────────────────────────

        var calendarTodos = todos
            .Where(t => t.DueDate.HasValue)
            .Select(t => new CalendarTodoDto
            {
                Id           = t.Id,
                Title        = t.Title,
                Notes        = t.Notes,
                DueDate      = t.DueDate,
                Priority     = (int)t.Priority,
                PriorityName = FormatEnum(t.Priority.ToString()),
                IsCompleted  = t.IsCompleted,
                ApiaryId     = t.ApiaryId,
                ApiaryName   = t.ApiaryId.HasValue && apiaryNames.TryGetValue(t.ApiaryId.Value, out var an) ? an : null,
                BeehiveId    = t.BeehiveId,
                BeehiveName  = t.BeehiveId.HasValue && beehiveNames.TryGetValue(t.BeehiveId.Value, out var bn) ? bn : null,
            })
            .ToList();

        var calendarEntries = diets
            .SelectMany(d => d.FeedingEntries.Select(e => new CalendarFeedingEntryDto
            {
                Id            = e.Id,
                ScheduledDate = e.ScheduledDate,
                Status        = (int)e.Status,
                StatusName    = e.Status.ToString(),
                DietId        = d.Id,
                DietName      = d.Name,
                BeehiveId     = d.BeehiveId,
                BeehiveName   = beehiveNames.TryGetValue(d.BeehiveId, out var bName) ? bName : $"Beehive {d.BeehiveId}",
                FoodTypeName  = d.FoodType == FoodType.Custom
                    ? (d.CustomFoodType ?? "Custom")
                    : FormatEnum(d.FoodType.ToString()),
            }))
            .ToList();

        return new CalendarEventsDto
        {
            Todos          = calendarTodos,
            FeedingEntries = calendarEntries,
        };
    }

    private static string FormatEnum(string raw)
    {
        var result = new System.Text.StringBuilder();
        for (int i = 0; i < raw.Length; i++)
        {
            if (i > 0 && char.IsUpper(raw[i]))
                result.Append(' ');
            result.Append(raw[i]);
        }
        return result.ToString();
    }
}
