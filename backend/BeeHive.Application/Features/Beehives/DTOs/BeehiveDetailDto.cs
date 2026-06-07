using BeeHive.Application.Features.Inspections.DTOs;

namespace BeeHive.Application.Features.Beehives.DTOs;

/// <summary>Full beehive representation including its inspections.</summary>
public class BeehiveDetailDto : BeehiveDto
{
    public IEnumerable<InspectionDto> Inspections { get; set; } = new List<InspectionDto>();
}
