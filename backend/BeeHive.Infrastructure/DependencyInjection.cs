using BeeHive.Application.Common.Interfaces;
using BeeHive.Infrastructure.Alerts;
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

        // Notification emails are delivered by a background worker so SMTP never
        // blocks (or times out) the HTTP request that produced the notification.
        services.AddSingleton<IEmailQueue, ChannelEmailQueue>();
        services.AddHostedService<EmailNotificationWorker>();

        // Daily proactive alert scan + weekly AI summary (SPEC-04).
        services.AddHostedService<AlertScanWorker>();

        return services;
    }
}
