using BeeHive.Application.Common.Exceptions;
using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Common.Security;
using BeeHive.Application.Features.Harvests;
using BeeHive.Application.Features.Harvests.DTOs;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using NSubstitute;
using Xunit;

namespace BeeHive.Application.Tests;

/// <summary>Locks the harvest rules: entries must belong to the chosen apiary (→ 400), and a
/// Beekeeper only ever sees harvests that contain one of their assigned hives (SPEC-02).</summary>
public class HarvestServiceTests
{
    private const int ApiaryId = 1;

    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IAccessGuard _access = Substitute.For<IAccessGuard>();

    private static Beehive Hive(int id) => new() { Id = id, Name = $"Košnica {id}", ApiaryId = ApiaryId };

    private static Harvest HarvestWith(int id, params int[] hiveIds) => new()
    {
        Id = id, ApiaryId = ApiaryId, Date = DateTime.UtcNow, HoneyType = HoneyType.Acacia,
        Entries = hiveIds.Select(h => new HarvestEntry { BeehiveId = h, QuantityKg = 5m }).ToList(),
    };

    private HarvestService Service(ICurrentUser user) => new(_uow, _access, user);

    [Fact]
    public async Task Create_WithHiveNotInApiary_ThrowsValidation()
    {
        var user = new TestCurrentUser { UserId = 1, Role = UserRole.OrganizationAdmin, OrganizationId = 1 };
        _uow.Beehives.GetByApiaryIdAsync(ApiaryId).Returns(new[] { Hive(5) }); // apiary only has hive 5

        var dto = new CreateHarvestDto
        {
            ApiaryId = ApiaryId, Date = DateTime.UtcNow, HoneyType = HoneyType.Acacia,
            Entries = [new CreateHarvestEntryDto { BeehiveId = 99, QuantityKg = 3m }], // hive 99 not in apiary
        };

        await Assert.ThrowsAsync<ValidationException>(() => Service(user).CreateAsync(dto));
        await _uow.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task GetAll_AsBeekeeper_ReturnsOnlyHarvestsContainingAssignedHives()
    {
        var user = new TestCurrentUser { UserId = 7, Role = UserRole.Beekeeper, OrganizationId = 1 };
        _access.GetAssignedBeehiveIdsAsync().Returns(new HashSet<int> { 5 });
        _access.GetAssignedApiaryIdsAsync().Returns(new HashSet<int> { ApiaryId });
        _uow.Harvests.GetByApiaryAsync(ApiaryId, Arg.Any<int?>())
            .Returns(new[] { HarvestWith(1, 5, 6), HarvestWith(2, 8, 9) }); // only #1 has hive 5

        var result = (await Service(user).GetAllAsync(null, null, null)).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetAll_AsBeekeeper_WithNoAssignments_ReturnsEmpty()
    {
        var user = new TestCurrentUser { UserId = 7, Role = UserRole.Beekeeper, OrganizationId = 1 };
        _access.GetAssignedBeehiveIdsAsync().Returns(new HashSet<int>());

        var result = await Service(user).GetAllAsync(null, null, null);

        Assert.Empty(result);
    }
}
