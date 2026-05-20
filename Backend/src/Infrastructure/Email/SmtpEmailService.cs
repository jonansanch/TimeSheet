using System.Net;
using System.Net.Mail;
using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KPG.Timesheet.Infrastructure.Email;

public class SmtpEmailService(IOptions<SmtpSettings> options, ILogger<SmtpEmailService> logger)
    : IEmailService
{
    private readonly SmtpSettings _settings = options.Value;

    public async Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.Host))
        {
            logger.LogWarning("SMTP no configurado — correo a {To} omitido.", to);
            return false;
        }

        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password)
            };

            using var message = new MailMessage(
                new MailAddress(_settings.FromAddress, _settings.FromName),
                new MailAddress(to))
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            await client.SendMailAsync(message, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar correo a {To}.", to);
            return false;
        }
    }
}
