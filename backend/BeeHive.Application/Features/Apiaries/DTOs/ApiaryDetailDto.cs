using BeeHive.Application.Features.Beehives.DTOs;

namespace BeeHive.Application.Features.Apiaries.DTOs;

/// <summary>Full apiary representation including its beehives.</summary>
public class ApiaryDetailDto : ApiaryDto
{
    public IEnumerable<BeehiveDto> Beehives { get; set; } = new List<BeehiveDto>();
}
