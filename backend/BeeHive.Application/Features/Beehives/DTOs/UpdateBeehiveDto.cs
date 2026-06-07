using BeeHive.Domain.Enums;

namespace BeeHive.Application.Features.Beehives.DTOs;

public class UpdateBeehiveDto
{
    public string Name { get; set; } = string.Empty;
    public BeehiveType Type { get; set; }
    public BeehiveMaterial Material { get; set; }
    public DateTime DateCreated { get; set; }
    public string? Notes { get; set; }
    public int ApiaryId { get; set; }
}
