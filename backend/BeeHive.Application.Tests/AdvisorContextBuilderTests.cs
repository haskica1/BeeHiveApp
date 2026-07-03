using BeeHive.Application.Features.Advisor;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Xunit;

namespace BeeHive.Application.Tests;

/// <summary>The hive-context block is the grounding the advisor answers from, so its rendering and
/// truncation must be faithful and deterministic (SPEC-01).</summary>
public class AdvisorContextBuilderTests
{
    private static Beehive Hive() => new()
    {
        Id = 1, Name = "Košnica 1", Type = BeehiveType.Langstroth, Material = BeehiveMaterial.Wood, ApiaryId = 3,
    };

    [Fact]
    public void Build_WithFullData_IncludesEverySection()
    {
        var inspections = new List<Inspection>
        {
            new() { Date = new DateTime(2026, 7, 1), HoneyLevel = HoneyLevel.High, BroodStatus = "Matica uočena", Notes = "Mirno društvo" },
        };
        var diet = new Diet { FoodType = FoodType.SugarSyrup, Status = DietStatus.InProgress };
        var todos = new List<Todo> { new() { Title = "Dodati nastavak", Priority = TodoPriority.High, DueDate = new DateTime(2026, 7, 10) } };
        var queen = new Queen { Year = 2024, Status = QueenStatus.Active, Origin = QueenOrigin.Purchased };

        var text = AdvisorContextBuilder.Build(
            Hive(), "Pčelinjak Sjever", inspections, diet, 1, 3, todos, queen, seasonYieldKg: 14.5m,
            weatherLine: "12°C trenutno, danas 8–22°C");

        Assert.Contains("Košnica 1", text);
        Assert.Contains("Pčelinjak Sjever", text);
        Assert.Contains("Matica: godina 2024", text);      // queen section
        Assert.Contains("Prinos meda ove sezone: 14.5 kg", text);
        Assert.Contains("med Visoko", text);                // inspection with Bosnian honey label
        Assert.Contains("Matica uočena", text);
        Assert.Contains("Aktivna prihrana: Šećerni sirup (1/3 obroka)", text);
        Assert.Contains("Dodati nastavak", text);
        Assert.Contains("Vrijeme na pčelinjaku: 12°C trenutno, danas 8–22°C", text);
    }

    [Fact]
    public void Build_WithoutData_StatesNoInspectionsAndOmitsOptionalSections()
    {
        var text = AdvisorContextBuilder.Build(
            Hive(), "Pčelinjak A", [], activeDiet: null, 0, 0, [], activeQueen: null,
            seasonYieldKg: null, weatherLine: null);

        Assert.Contains("Pregledi: nema zabilježenih pregleda.", text);
        Assert.DoesNotContain("Matica:", text);
        Assert.DoesNotContain("Aktivna prihrana", text);
        Assert.DoesNotContain("Prinos meda", text);
        Assert.DoesNotContain("Vrijeme", text);
    }

    [Fact]
    public void Build_TruncatesLongInspectionText()
    {
        var longNote = new string('x', 300);
        var inspections = new List<Inspection>
        {
            new() { Date = new DateTime(2026, 7, 1), HoneyLevel = HoneyLevel.Low, BroodStatus = longNote },
        };

        var text = AdvisorContextBuilder.Build(
            Hive(), "A", inspections, null, 0, 0, [], null, null, null);

        Assert.Contains("…", text);
        Assert.DoesNotContain(longNote, text); // full 300-char string must not appear verbatim
    }
}
