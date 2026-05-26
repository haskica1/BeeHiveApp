using System.Net;
using System.Net.Mail;
using BeeHive.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BeeHive.Infrastructure.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var host = _config["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogDebug("SMTP not configured — skipping email to {Email}", toEmail);
            return;
        }

        var port     = int.Parse(_config["Smtp:Port"] ?? "587");
        var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true");
        var username  = _config["Smtp:Username"] ?? string.Empty;
        var password  = _config["Smtp:Password"] ?? string.Empty;
        var fromEmail = _config["Smtp:FromEmail"] ?? "noreply@beehive.com";
        var fromName  = _config["Smtp:FromName"] ?? "BeeHive App";

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl   = enableSsl,
                Credentials = new NetworkCredential(username, password),
            };

            using var message = new MailMessage
            {
                From       = new MailAddress(fromEmail, fromName),
                Subject    = subject,
                Body       = htmlBody,
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            // Email failures must not break the main operation
            _logger.LogError(ex, "Failed to send email to {Email} — subject: {Subject}", toEmail, subject);
        }
    }
}
