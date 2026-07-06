using BeeHive.Application.Common.Interfaces;
using BeeHive.Application.Features.Auth;
using BeeHive.Application.Features.Auth.DTOs;
using BeeHive.Application.Features.Notifications;
using BeeHive.Domain.Entities;
using BeeHive.Domain.Enums;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using Xunit;

namespace BeeHive.Application.Tests;

/// <summary>
/// New organizations start on a 30-day Pro trial (SPEC-09) — implemented as a pre-set expiring
/// Pro plan, so no new machinery: the computed effective plan falls back to Free after expiry.
/// </summary>
public class RegistrationTrialTests
{
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly AuthService _service;

    public RegistrationTrialTests()
    {
        var config = Substitute.For<IConfiguration>();
        config["Jwt:Secret"].Returns("unit-test-secret-key-that-is-long-enough-123456");
        config["Jwt:Issuer"].Returns("BeeHiveTests");
        config["Jwt:Audience"].Returns("BeeHiveTests");
        config["Plans:Trial:Days"].Returns("30");

        _service = new AuthService(_uow, config, Substitute.For<INotificationService>());
    }

    [Fact]
    public async Task Register_CreatesOrganization_OnThirtyDayProTrial()
    {
        User? captured = null;
        _uow.Users.GetByEmailAsync(Arg.Any<string>()).Returns((User?)null);
        _uow.Users.AddAsync(Arg.Do<User>(u => captured = u)).Returns(ci => ci.Arg<User>());

        var before = DateTime.UtcNow.Date;
        await _service.RegisterAsync(new RegisterDto(
            "Asim", "Tester", "new@org.ba", "Correct123!", "Nova Organizacija", null));

        var org = captured!.Organization!;
        Assert.Equal(PlanType.Pro, org.Plan);
        Assert.Equal("Probni period", org.PlanNotes);
        Assert.NotNull(org.PlanValidUntil);
        Assert.Equal(before.AddDays(30), org.PlanValidUntil!.Value.Date);
    }
}
