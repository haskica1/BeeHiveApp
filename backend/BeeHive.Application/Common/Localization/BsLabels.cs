using BeeHive.Domain.Enums;

namespace BeeHive.Application.Common.Localization;

/// <summary>
/// Bosnian display labels for domain enums. These are the UI-facing strings the
/// API returns in <c>*Name</c> fields and in statistics, so the whole app shows
/// Bosnian text. Keep these in sync with the frontend label maps in
/// <c>frontend/src/core/models/index.ts</c>.
/// </summary>
public static class BsLabels
{
    public static string Label(FoodType ft) => ft switch
    {
        FoodType.SugarSyrup     => "Šećerni sirup",
        FoodType.Fondant        => "Fondan",
        FoodType.Pollen         => "Polen",
        FoodType.ProteinPatties => "Proteinski kolači",
        FoodType.Custom         => "Vlastito",
        _                       => ft.ToString(),
    };

    public static string Label(DietReason r) => r switch
    {
        DietReason.LackOfFood               => "Nedostatak hrane",
        DietReason.WinterFeeding            => "Zimsko hranjenje",
        DietReason.SpringStimulation        => "Proljetna stimulacija",
        DietReason.NewSwarmSupport          => "Podrška novom roju",
        DietReason.PostHarvestRecovery      => "Oporavak nakon berbe",
        DietReason.DroughtConditions        => "Uvjeti suše",
        DietReason.WeakColonySupport        => "Podrška slaboj koloniji",
        DietReason.QueenIntroductionSupport => "Podrška uvođenju matice",
        DietReason.Custom                   => "Vlastito",
        _                                   => r.ToString(),
    };

    public static string Label(DietStatus s) => s switch
    {
        DietStatus.NotStarted   => "Nije započeto",
        DietStatus.InProgress   => "U toku",
        DietStatus.Completed    => "Završeno",
        DietStatus.StoppedEarly => "Prijevremeno prekinuto",
        _                       => s.ToString(),
    };

    public static string Label(FeedingEntryStatus s) => s switch
    {
        FeedingEntryStatus.Pending   => "Na čekanju",
        FeedingEntryStatus.Completed => "Završeno",
        _                            => s.ToString(),
    };

    public static string Label(HoneyLevel h) => h switch
    {
        HoneyLevel.Low    => "Nisko",
        HoneyLevel.Medium => "Srednje",
        HoneyLevel.High   => "Visoko",
        _                 => h.ToString(),
    };

    public static string Label(TodoPriority p) => p switch
    {
        TodoPriority.Low    => "Nizak",
        TodoPriority.Medium => "Srednji",
        TodoPriority.High   => "Visok",
        _                   => p.ToString(),
    };

    public static string Label(BeehiveType t) => t switch
    {
        BeehiveType.Langstroth  => "LR (Langstroth-Rutova) košnica",
        BeehiveType.DadantBlatt => "DB (Dadan-Blatt) košnica",
        BeehiveType.Warré       => "AŽ (Alberti-Žnideršič) košnica",
        BeehiveType.TopBar      => "Pološka košnica",
        BeehiveType.Other       => "Ostalo",
        _                       => t.ToString(),
    };

    public static string Label(QueenMarkColor c) => c switch
    {
        QueenMarkColor.White  => "Bijela",
        QueenMarkColor.Yellow => "Žuta",
        QueenMarkColor.Red    => "Crvena",
        QueenMarkColor.Green  => "Zelena",
        QueenMarkColor.Blue   => "Plava",
        _                     => c.ToString(),
    };

    public static string Label(QueenOrigin o) => o switch
    {
        QueenOrigin.Purchased   => "Kupljena",
        QueenOrigin.OwnBreeding => "Vlastiti uzgoj",
        QueenOrigin.Swarm       => "Rojenje",
        QueenOrigin.Supersedure => "Tiha zamjena",
        QueenOrigin.Unknown     => "Nepoznato",
        _                       => o.ToString(),
    };

    public static string Label(QueenStatus s) => s switch
    {
        QueenStatus.Active   => "Aktivna",
        QueenStatus.Replaced => "Zamijenjena",
        QueenStatus.Died     => "Uginula",
        QueenStatus.Missing  => "Nestala",
        _                    => s.ToString(),
    };

    public static string Label(BeehiveMaterial m) => m switch
    {
        BeehiveMaterial.Wood        => "Drvo",
        BeehiveMaterial.Plastic     => "Plastika",
        BeehiveMaterial.Polystyrene => "Stiropor",
        _                           => m.ToString(),
    };

    private static readonly string[] MonthsShort =
        { "jan", "feb", "mar", "apr", "maj", "jun", "jul", "avg", "sep", "okt", "nov", "dec" };

    /// <summary>Short Bosnian month label with 2-digit year, e.g. "maj 25".</summary>
    public static string MonthShort(int year, int month) =>
        $"{MonthsShort[month - 1]} {year % 100:00}";
}
