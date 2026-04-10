using BeeHive.Domain.Common;

namespace BeeHive.Domain.Entities;

/// <summary>
/// Represents an apiary (pčelinjak) — a location containing multiple beehives.
/// </summary>
public class Apiary : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>WGS-84 latitude (-90 to 90). Null when no location has been set.</summary>
    public double? Latitude { get; set; }

    /// <summary>WGS-84 longitude (-180 to 180). Null when no location has been set.</summary>
    public double? Longitude { get; set; }

    // Navigation property
    public ICollection<Beehive> Beehives { get; set; } = new List<Beehive>();
}
