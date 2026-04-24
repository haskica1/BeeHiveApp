using AutoMapper;
using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Diets.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Diets;

// ── Interface ─────────────────────────────────────────────────────────────────

public interface IDietService
{
    Task<IEnumerable<DietDto>> GetByBeehiveIdAsync(int beehiveId);
    Task<DietDetailDto> GetByIdAsync(int id);
    Task<DietDetailDto> CreateAsync(CreateDietDto dto, int? createdById);
    Task<DietDetailDto> UpdateAsync(int id, UpdateDietDto dto);
    Task DeleteAsync(int id);
    Task<DietDetailDto> CompleteEarlyAsync(int id, CompleteEarlyDto dto);
    Task<FeedingEntryDto> CompleteFeedingEntryAsync(int dietId, int entryId);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class DietService : IDietService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DietService(IUnitOfWork uow, IMapper mapper)
    {
        _uow    = uow;
        _mapper = mapper;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<DietDto>> GetByBeehiveIdAsync(int beehiveId)
    {
        if (!await _uow.Beehives.ExistsAsync(beehiveId))
            throw new NotFoundException(nameof(Beehive), beehiveId);

        var diets = await _uow.Diets.GetByBeehiveIdAsync(beehiveId);

        return diets.Select(d => MapToDietDto(d));
    }

    public async Task<DietDetailDto> GetByIdAsync(int id)
    {
        var diet = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new NotFoundException(nameof(Diet), id);

        return MapToDietDetailDto(diet);
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    public async Task<DietDetailDto> CreateAsync(CreateDietDto dto, int? createdById)
    {
        if (!await _uow.Beehives.ExistsAsync(dto.BeehiveId))
            throw new NotFoundException(nameof(Beehive), dto.BeehiveId);

        var diet = new Diet
        {
            Name         = dto.Name,
            StartDate    = dto.StartDate.Date,
            Reason       = dto.Reason,
            CustomReason = dto.CustomReason,
            DurationDays = dto.DurationDays,
            FrequencyDays = dto.FrequencyDays,
            FoodType     = dto.FoodType,
            CustomFoodType = dto.CustomFoodType,
            BeehiveId    = dto.BeehiveId,
            Status       = CalculateInitialStatus(dto.StartDate),
            CreatedById  = createdById,
        };

        diet.FeedingEntries = GenerateEntries(diet.StartDate, dto.DurationDays, dto.FrequencyDays);

        await _uow.Diets.AddAsync(diet);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Diets.GetWithEntriesAsync(diet.Id)
            ?? throw new Exception("Failed to reload diet after creation.");

        return MapToDietDetailDto(saved);
    }

    public async Task<DietDetailDto> UpdateAsync(int id, UpdateDietDto dto)
    {
        var diet = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new NotFoundException(nameof(Diet), id);

        if (diet.Status == DietStatus.Completed || diet.Status == DietStatus.StoppedEarly)
            throw new BusinessRuleException("A completed or stopped diet cannot be updated.");

        diet.Name          = dto.Name;
        diet.StartDate     = dto.StartDate.Date;
        diet.Reason        = dto.Reason;
        diet.CustomReason  = dto.CustomReason;
        diet.DurationDays  = dto.DurationDays;
        diet.FrequencyDays = dto.FrequencyDays;
        diet.FoodType      = dto.FoodType;
        diet.CustomFoodType = dto.CustomFoodType;
        diet.UpdatedAt     = DateTime.UtcNow;

        // Recalculate entries: keep completed, replace all pending
        RecalculateEntries(diet);

        // Recalculate status
        diet.Status = CalculateStatus(diet);

        await _uow.Diets.UpdateAsync(diet);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new Exception("Failed to reload diet after update.");

        return MapToDietDetailDto(saved);
    }

    public async Task DeleteAsync(int id)
    {
        var diet = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new NotFoundException(nameof(Diet), id);

        var today = DateTime.UtcNow.Date;
        var hasCompletedEntries = diet.FeedingEntries.Any(e => e.Status == FeedingEntryStatus.Completed);
        var hasStarted = diet.StartDate.Date <= today;

        if (hasStarted || hasCompletedEntries)
            throw new BusinessRuleException(
                "A diet can only be deleted before it has started (no completed feedings and start date not yet reached).");

        await _uow.Diets.DeleteAsync(diet);
        await _uow.SaveChangesAsync();
    }

    public async Task<DietDetailDto> CompleteEarlyAsync(int id, CompleteEarlyDto dto)
    {
        var diet = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new NotFoundException(nameof(Diet), id);

        if (diet.Status == DietStatus.Completed || diet.Status == DietStatus.StoppedEarly)
            throw new BusinessRuleException("Diet is already finished.");

        if (string.IsNullOrWhiteSpace(dto.Comment))
            throw new BusinessRuleException("A comment is required when completing a diet early.");

        diet.Status                  = DietStatus.StoppedEarly;
        diet.EarlyCompletionComment  = dto.Comment;
        diet.UpdatedAt               = DateTime.UtcNow;

        await _uow.Diets.UpdateAsync(diet);
        await _uow.SaveChangesAsync();

        var saved = await _uow.Diets.GetWithEntriesAsync(id)
            ?? throw new Exception("Failed to reload diet after completing early.");
        return MapToDietDetailDto(saved);
    }

    public async Task<FeedingEntryDto> CompleteFeedingEntryAsync(int dietId, int entryId)
    {
        var diet = await _uow.Diets.GetWithEntriesAsync(dietId)
            ?? throw new NotFoundException(nameof(Diet), dietId);

        if (diet.Status == DietStatus.Completed || diet.Status == DietStatus.StoppedEarly)
            throw new BusinessRuleException("Cannot mark feedings on a finished diet.");

        var entry = diet.FeedingEntries.FirstOrDefault(e => e.Id == entryId)
            ?? throw new NotFoundException(nameof(FeedingEntry), entryId);

        if (entry.Status == FeedingEntryStatus.Completed)
            throw new BusinessRuleException("This feeding entry is already completed.");

        entry.Status         = FeedingEntryStatus.Completed;
        entry.CompletionDate = DateTime.UtcNow;
        entry.UpdatedAt      = DateTime.UtcNow;

        // If all entries are now done, mark the diet as completed
        if (diet.FeedingEntries.All(e => e.Status == FeedingEntryStatus.Completed))
        {
            diet.Status    = DietStatus.Completed;
            diet.UpdatedAt = DateTime.UtcNow;
        }
        else if (diet.Status == DietStatus.NotStarted)
        {
            diet.Status    = DietStatus.InProgress;
            diet.UpdatedAt = DateTime.UtcNow;
        }

        await _uow.Diets.UpdateAsync(diet);
        await _uow.SaveChangesAsync();

        return MapToFeedingEntryDto(entry);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates feeding entries for a schedule starting at <paramref name="startDate"/>
    /// over <paramref name="durationDays"/> with one feeding every <paramref name="frequencyDays"/> days.
    /// </summary>
    private static List<FeedingEntry> GenerateEntries(DateTime startDate, int durationDays, int frequencyDays)
    {
        var entries = new List<FeedingEntry>();
        int count = durationDays / frequencyDays; // integer division
        if (count < 1) count = 1;

        for (int i = 0; i < count; i++)
        {
            entries.Add(new FeedingEntry
            {
                ScheduledDate = startDate.Date.AddDays(i * frequencyDays),
                Status        = FeedingEntryStatus.Pending,
            });
        }
        return entries;
    }

    /// <summary>
    /// Rebuilds pending feeding entries after a diet update, preserving completed ones.
    /// </summary>
    private static void RecalculateEntries(Diet diet)
    {
        // Gather completed entry dates to avoid duplicating them
        var completedDates = diet.FeedingEntries
            .Where(e => e.Status == FeedingEntryStatus.Completed)
            .Select(e => e.ScheduledDate.Date)
            .ToHashSet();

        // Remove all pending entries (EF Core will handle DELETE via cascade)
        var pending = diet.FeedingEntries.Where(e => e.Status == FeedingEntryStatus.Pending).ToList();
        foreach (var p in pending)
            diet.FeedingEntries.Remove(p);

        // Generate the full schedule and add entries for dates not already completed
        int count = diet.DurationDays / diet.FrequencyDays;
        if (count < 1) count = 1;

        for (int i = 0; i < count; i++)
        {
            var date = diet.StartDate.Date.AddDays(i * diet.FrequencyDays);
            if (!completedDates.Contains(date))
            {
                diet.FeedingEntries.Add(new FeedingEntry
                {
                    ScheduledDate = date,
                    Status        = FeedingEntryStatus.Pending,
                    DietId        = diet.Id,
                });
            }
        }
    }

    private static DietStatus CalculateInitialStatus(DateTime startDate)
        => startDate.Date <= DateTime.UtcNow.Date
            ? DietStatus.InProgress
            : DietStatus.NotStarted;

    private static DietStatus CalculateStatus(Diet diet)
    {
        if (diet.Status == DietStatus.StoppedEarly) return DietStatus.StoppedEarly;

        var allCompleted = diet.FeedingEntries.Count > 0
                           && diet.FeedingEntries.All(e => e.Status == FeedingEntryStatus.Completed);

        if (allCompleted) return DietStatus.Completed;

        return diet.StartDate.Date <= DateTime.UtcNow.Date
            ? DietStatus.InProgress
            : DietStatus.NotStarted;
    }

    // ── Mapping helpers (avoid AutoMapper for computed props) ─────────────────

    private static DietDto MapToDietDto(Diet d) => new()
    {
        Id                     = d.Id,
        Name                   = d.Name,
        StartDate              = d.StartDate,
        Reason                 = d.Reason,
        ReasonName             = FormatReason(d.Reason),
        CustomReason           = d.CustomReason,
        DurationDays           = d.DurationDays,
        FrequencyDays          = d.FrequencyDays,
        FoodType               = d.FoodType,
        FoodTypeName           = FormatFoodType(d.FoodType),
        CustomFoodType         = d.CustomFoodType,
        Status                 = d.Status,
        StatusName             = d.Status.ToString(),
        EarlyCompletionComment = d.EarlyCompletionComment,
        BeehiveId              = d.BeehiveId,
        TotalEntries           = d.FeedingEntries.Count,
        CompletedEntries       = d.FeedingEntries.Count(e => e.Status == FeedingEntryStatus.Completed),
        CreatedByName          = d.CreatedBy != null ? $"{d.CreatedBy.FirstName} {d.CreatedBy.LastName}" : null,
        CreatedAt              = d.CreatedAt,
    };

    private static DietDetailDto MapToDietDetailDto(Diet d) => new()
    {
        Id                     = d.Id,
        Name                   = d.Name,
        StartDate              = d.StartDate,
        Reason                 = d.Reason,
        ReasonName             = FormatReason(d.Reason),
        CustomReason           = d.CustomReason,
        DurationDays           = d.DurationDays,
        FrequencyDays          = d.FrequencyDays,
        FoodType               = d.FoodType,
        FoodTypeName           = FormatFoodType(d.FoodType),
        CustomFoodType         = d.CustomFoodType,
        Status                 = d.Status,
        StatusName             = d.Status.ToString(),
        EarlyCompletionComment = d.EarlyCompletionComment,
        BeehiveId              = d.BeehiveId,
        TotalEntries           = d.FeedingEntries.Count,
        CompletedEntries       = d.FeedingEntries.Count(e => e.Status == FeedingEntryStatus.Completed),
        CreatedByName          = d.CreatedBy != null ? $"{d.CreatedBy.FirstName} {d.CreatedBy.LastName}" : null,
        CreatedAt              = d.CreatedAt,
        FeedingEntries         = d.FeedingEntries
            .OrderBy(e => e.ScheduledDate)
            .Select(MapToFeedingEntryDto)
            .ToList(),
    };

    private static FeedingEntryDto MapToFeedingEntryDto(FeedingEntry e) => new()
    {
        Id             = e.Id,
        ScheduledDate  = e.ScheduledDate,
        Status         = e.Status,
        StatusName     = e.Status.ToString(),
        CompletionDate = e.CompletionDate,
        DietId         = e.DietId,
    };

    private static string FormatReason(DietReason r) => r switch
    {
        DietReason.LackOfFood               => "Lack of Food",
        DietReason.WinterFeeding            => "Winter Feeding",
        DietReason.SpringStimulation        => "Spring Stimulation",
        DietReason.NewSwarmSupport          => "New Swarm Support",
        DietReason.PostHarvestRecovery      => "Post-Harvest Recovery",
        DietReason.DroughtConditions        => "Drought Conditions",
        DietReason.WeakColonySupport        => "Weak Colony Support",
        DietReason.QueenIntroductionSupport => "Queen Introduction Support",
        DietReason.Custom                   => "Custom",
        _                                   => r.ToString(),
    };

    private static string FormatFoodType(FoodType ft) => ft switch
    {
        FoodType.SugarSyrup     => "Sugar Syrup",
        FoodType.Fondant        => "Fondant",
        FoodType.Pollen         => "Pollen",
        FoodType.ProteinPatties => "Protein Patties",
        FoodType.Custom         => "Custom",
        _                       => ft.ToString(),
    };
}
