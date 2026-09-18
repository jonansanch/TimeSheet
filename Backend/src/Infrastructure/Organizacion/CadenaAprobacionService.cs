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
        string cliente,
        string proyecto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return CadenaAprobacionDto.Vacia;

        var perfil = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.PuestoId, u.SupervisorUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (perfil is null)
            return CadenaAprobacionDto.Vacia;

        var nombreCliente  = cliente?.Trim()  ?? string.Empty;
        var nombreProyecto = proyecto?.Trim() ?? string.Empty;

        var clienteId = await context.Clientes
            .Where(c => c.Nombre == nombreCliente)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new CadenaAprobacionDto(
            await ResolverSupervisorPuestoAsync(perfil.PuestoId, clienteId, cancellationToken),
            perfil.SupervisorUserId,
            await ResolverSupervisorProyectoAsync(clienteId, nombreProyecto, cancellationToken));
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

    /// <summary>
    /// Tercera aprobacion. El proyecto se busca dentro del cliente porque su nombre
    /// solo es unico por cliente.
    /// </summary>
    private async Task<string?> ResolverSupervisorProyectoAsync(
        int? clienteId,
        string nombreProyecto,
        CancellationToken cancellationToken)
    {
        if (clienteId is null || string.IsNullOrWhiteSpace(nombreProyecto)) return null;

        return await context.Proyectos
            .Where(p => p.ClienteId == clienteId && p.Nombre == nombreProyecto)
            .Select(p => p.SupervisorUserId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
