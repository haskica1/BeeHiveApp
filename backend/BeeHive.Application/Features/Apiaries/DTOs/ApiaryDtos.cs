using BeeHive.Application.Features.Beehives.DTOs;

namespace BeeHive.Application.Features.Apiaries.DTOs;

/// <summary>Lightweight apiary representation for list views.</summary>
public class ApiaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool HasLocation => Latitude.HasValue && Longitude.HasValue;
    public int BeehiveCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Full apiary representation including its beehives.</summary>
public class ApiaryDetailDto : ApiaryDto
{
    public IEnumerable<BeehiveDto> Beehives { get; set; } = new List<BeehiveDto>();
}

public class CreateApiaryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class UpdateApiaryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
