using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Infrastructure.Notificaciones;

public class NotificadorAprobacion(
    IIdentityService identityService,
    IEmailService emailService,
    ILogger<NotificadorAprobacion> logger)
    : INotificadorAprobacion
{
    public async Task NotificarRechazoAsync(
        string empleadoUserId,
        DateOnly fechaRegistro,
        string proyecto,
        string comentario,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var emails = await identityService.GetUserEmailsAsync([empleadoUserId], cancellationToken);
            if (!emails.TryGetValue(empleadoUserId, out var email) || string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning(
                    "No se pudo avisar del rechazo: el usuario {UserId} no tiene email.", empleadoUserId);
                return;
            }

            var asunto = $"Timesheet KPG — registro rechazado ({fechaRegistro:dd/MM/yyyy})";
            var cuerpo = $"""
                Tu registro del {fechaRegistro:dd/MM/yyyy} en el proyecto "{proyecto}" fue rechazado.

                Motivo:
                {comentario}

                Corrigelo desde "Mis registros" y vuelve a enviarlo. La revision
                recomenzara desde el primer nivel.
                """;

            var enviado = await emailService.SendAsync(email, asunto, cuerpo, cancellationToken);
            if (!enviado)
                logger.LogWarning("El aviso de rechazo a {Email} no pudo enviarse.", email);
        }
        catch (Exception ex)
        {
            // Deliberado: el rechazo ya se guardo y debe mantenerse aunque el correo falle.
            logger.LogError(ex,
                "Fallo al avisar del rechazo al usuario {UserId} para la fecha {Fecha}.",
                empleadoUserId, fechaRegistro);
        }
    }
}
