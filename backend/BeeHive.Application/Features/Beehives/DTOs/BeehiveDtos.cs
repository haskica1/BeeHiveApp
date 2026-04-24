using BeeHive.Application.Features.Inspections.DTOs;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Beehives.DTOs;

/// <summary>Lightweight beehive representation for list/summary views.</summary>
public class BeehiveDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BeehiveType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public BeehiveMaterial Material { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public string? Notes { get; set; }
    public int ApiaryId { get; set; }
    public int InspectionCount { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UniqueId { get; set; }
    public string? QrCodeBase64 { get; set; }
}

/// <summary>Full beehive representation including its inspections.</summary>
public class BeehiveDetailDto : BeehiveDto
{
    public IEnumerable<InspectionDto> Inspections { get; set; } = new List<InspectionDto>();
}

public class CreateBeehiveDto
{
    public string Name { get; set; } = string.Empty;
    public BeehiveType Type { get; set; }
    public BeehiveMaterial Material { get; set; }
    public DateTime DateCreated { get; set; }
    public string? Notes { get; set; }
    public int ApiaryId { get; set; }
}

public class UpdateBeehiveDto
{
    public string Name { get; set; } = string.Empty;
    public BeehiveType Type { get; set; }
    public BeehiveMaterial Material { get; set; }
    public DateTime DateCreated { get; set; }
    public string? Notes { get; set; }
    public int ApiaryId { get; set; }
}
