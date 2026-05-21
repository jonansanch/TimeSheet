using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace KPG.Timesheet.Infrastructure.Bitacora;

public class BitacoraService(
    IApplicationDbContext context,
    TimeProvider timeProvider,
    ILogger<BitacoraService> logger) : IBitacoraService
{
    public async Task RegistrarAsync(
        string tipoEvento,
        string actorId,
        string? actorEmail,
        string entidadAfectada,
        string? entidadId,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entrada = BitacoraAuditoria.Crear(
                tipoEvento, actorId, actorEmail,
                entidadAfectada, entidadId, metadata,
                timeProvider.GetUtcNow());

            context.BitacoraAuditoria.Add(entrada);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al registrar en bitácora: TipoEvento={TipoEvento}, ActorId={ActorId}",
                tipoEvento, actorId);
        }
    }
}
