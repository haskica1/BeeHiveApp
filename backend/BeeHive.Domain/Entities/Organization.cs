using BeeHive.Domain.Common;

namespace BeeHive.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Apiary> Apiaries { get; set; } = new List<Apiary>();
}
