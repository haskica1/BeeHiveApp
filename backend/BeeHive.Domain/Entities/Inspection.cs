using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

/// <summary>
/// Represents a single inspection (pregled) of a beehive at a specific point in time.
/// </summary>
public class Inspection : BaseEntity
{
    public DateTime Date { get; set; }

    /// <summary>Ambient temperature in Celsius at the time of inspection.</summary>
    public double? Temperature { get; set; }

    public HoneyLevel HoneyLevel { get; set; }

    /// <summary>Free-text description of the brood status (e.g. "Healthy", "Queen seen", "No brood").</summary>
    public string? BroodStatus { get; set; }

    public string? Notes { get; set; }

    // Foreign key
    public int BeehiveId { get; set; }

    // Navigation property
    public Beehive Beehive { get; set; } = null!;
}
