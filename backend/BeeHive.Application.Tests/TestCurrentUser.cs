using BeeHive.Application.Common.Interfaces;
using BeeHive.Domain.Enums;

namespace BeeHive.Application.Tests;

/// <summary>Init-only <see cref="ICurrentUser"/> stand-in for authorization tests.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public int? UserId { get; init; }
    public UserRole? Role { get; init; }
    public int? OrganizationId { get; init; }
    public int? ApiaryId { get; init; }
    public bool IsAuthenticated => UserId.HasValue;
}
