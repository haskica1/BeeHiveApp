using BeeHive.Application.Common.Interfaces;
using BeeHive.Infrastructure.Email;
using Microsoft.Extensions.DependencyInjection;

namespace BeeHive.Infrastructure;

/// <summary>
/// Registers Infrastructure-layer services — external integrations such as email delivery.
/// Persistence lives in the <c>BeeHive.Entity</c> project (see <c>AddEntity</c>).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEmailService, EmailService>();
        return services;
    }
}
