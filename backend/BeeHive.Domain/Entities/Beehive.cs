using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

/// <summary>
/// Represents a single beehive (košnica) within an apiary.
/// </summary>
public class Beehive : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public BeehiveType Type { get; set; }
    public BeehiveMaterial Material { get; set; }
    public DateTime DateCreated { get; set; }
    public string? Notes { get; set; }

    /// <summary>Permanent, globally-unique identifier for this hive. Encoded into the QR code.</summary>
    public Guid? UniqueId { get; set; }

    /// <summary>PNG QR code image stored as a Base64 string. Generated once on creation.</summary>
    public string? QrCodeBase64 { get; set; }

    public int? CreatedById { get; set; }
    public User? CreatedBy { get; set; }

    // Foreign key
    public int ApiaryId { get; set; }

    // Navigation properties
    public Apiary Apiary { get; set; } = null!;
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Diet> Diets { get; set; } = new List<Diet>();
}
