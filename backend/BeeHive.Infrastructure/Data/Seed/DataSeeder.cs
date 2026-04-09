using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeeHive.Infrastructure.Data.Seed;

/// <summary>
/// Provides deterministic seed data for development and initial deployment.
/// Uses static IDs so migrations are idempotent.
/// </summary>
public static class DataSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Apiaries ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Apiary>().HasData(
            new Apiary
            {
                Id = 1,
                Name = "Gorska Pčelinja",
                Description = "Mountain apiary located near the forest edge, known for acacia and linden honey.",
                CreatedAt = now
            },
            new Apiary
            {
                Id = 2,
                Name = "Dolinska Farma",
                Description = "Valley farm apiary with diverse flora — clover, sunflower, and wildflower.",
                CreatedAt = now
            }
        );

        // ── Beehives ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Beehive>().HasData(
            new Beehive
            {
                Id = 1,
                Name = "Košnica A1",
                Type = BeehiveType.Langstroth,
                Material = BeehiveMaterial.Wood,
                DateCreated = new DateTime(2022, 3, 15),
                Notes = "Strong colony, productive queen introduced spring 2023.",
                ApiaryId = 1,
                CreatedAt = now
            },
            new Beehive
            {
                Id = 2,
                Name = "Košnica A2",
                Type = BeehiveType.DadantBlatt,
                Material = BeehiveMaterial.Wood,
                DateCreated = new DateTime(2022, 5, 20),
                Notes = "Newer colony, monitoring for development.",
                ApiaryId = 1,
                CreatedAt = now
            },
            new Beehive
            {
                Id = 3,
                Name = "Košnica B1",
                Type = BeehiveType.Langstroth,
                Material = BeehiveMaterial.Polystyrene,
                DateCreated = new DateTime(2023, 4, 10),
                Notes = "Insulated polystyrene hive — excellent for winter survival.",
                ApiaryId = 2,
                CreatedAt = now
            }
        );

        // ── Inspections ───────────────────────────────────────────────────────
        modelBuilder.Entity<Inspection>().HasData(
            new Inspection
            {
                Id = 1,
                Date = new DateTime(2024, 5, 10),
                Temperature = 22.5,
                HoneyLevel = HoneyLevel.High,
                BroodStatus = "Healthy brood pattern. Queen spotted. Eggs and larvae present.",
                Notes = "Colony strong. Added super for honey storage.",
                BeehiveId = 1,
                CreatedAt = now
            },
            new Inspection
            {
                Id = 2,
                Date = new DateTime(2024, 6, 15),
                Temperature = 28.0,
                HoneyLevel = HoneyLevel.Medium,
                BroodStatus = "Good brood. Some drone cells observed.",
                Notes = "Honey super 60% full. Will harvest next visit.",
                BeehiveId = 1,
                CreatedAt = now
            },
            new Inspection
            {
                Id = 3,
                Date = new DateTime(2024, 5, 12),
                Temperature = 21.0,
                HoneyLevel = HoneyLevel.Low,
                BroodStatus = "Sparse brood. Queen activity low.",
                Notes = "Consider requeening if no improvement in 3 weeks.",
                BeehiveId = 2,
                CreatedAt = now
            },
            new Inspection
            {
                Id = 4,
                Date = new DateTime(2024, 6, 1),
                Temperature = 25.5,
                HoneyLevel = HoneyLevel.Medium,
                BroodStatus = "Improving brood pattern. Queen productive.",
                Notes = "Colony recovering well.",
                BeehiveId = 3,
                CreatedAt = now
            }
        );
    }
}
