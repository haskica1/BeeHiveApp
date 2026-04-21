using BeeHive.Domain.Common;
using BeeHive.Domain.Enums;

namespace BeeHive.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Admin;

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
}
