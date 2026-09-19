using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.Organizacion;

/// <summary>
/// Vive en Infrastructure porque necesita AspNetUsers, que Application no expone.
/// </summary>
public class CadenaAprobacionService(ApplicationDbContext context) : ICadenaAprobacionService
{
    public async Task<CadenaAprobacionDto> ResolverAsync(
        string userId,
        int proyectoId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || proyectoId <= 0)
            return CadenaAprobacionDto.Vacia;

        var perfil = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.PuestoId, u.SupervisorUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (perfil is null)
            return CadenaAprobacionDto.Vacia;

        // Desde la migracion a ProyectoId el cliente sale del propio proyecto:
        // ya no hace falta buscarlo por nombre.
        var proyecto = await context.Proyectos
            .Where(p => p.Id == proyectoId)
            .Select(p => new { p.ClienteId, p.SupervisorUserId })
            .FirstOrDefaultAsync(cancellationToken);

        return new CadenaAprobacionDto(
            await ResolverSupervisorPuestoAsync(perfil.PuestoId, proyecto?.ClienteId, cancellationToken),
            perfil.SupervisorUserId,
            proyecto?.SupervisorUserId);
    }

    /// <summary>
    /// Primera aprobacion. La regla especifica del cliente gana sobre la general;
    /// si no hay ninguna especifica, cae a la que no distingue cliente.
    /// </summary>
    private async Task<string?> ResolverSupervisorPuestoAsync(
        int? puestoId,
        int? clienteId,
        CancellationToken cancellationToken)
    {
        if (puestoId is null) return null;

        var reglas = await context.SupervisoresPuesto
            .Where(s => s.Activo
                     && s.PuestoId == puestoId
                     && (s.ClienteId == null || s.ClienteId == clienteId))
            .Select(s => new { s.ClienteId, s.SupervisorUserId })
            .ToListAsync(cancellationToken);

        return reglas
            .OrderByDescending(r => r.ClienteId.HasValue)   // especifica primero
            .Select(r => r.SupervisorUserId)
            .FirstOrDefault();
    }
}
