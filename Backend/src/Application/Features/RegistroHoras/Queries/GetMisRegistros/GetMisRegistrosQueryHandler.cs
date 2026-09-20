using KPG.Timesheet.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Queries.GetMisRegistros;

public class GetMisRegistrosQueryHandler : IRequestHandler<GetMisRegistrosQuery, MisRegistrosPaginadosResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identity;

    public GetMisRegistrosQueryHandler(
        IApplicationDbContext context, IUser user, IIdentityService identity)
    {
        _context  = context;
        _user     = user;
        _identity = identity;
    }

    public async Task<MisRegistrosPaginadosResponse> Handle(
        GetMisRegistrosQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            return new MisRegistrosPaginadosResponse(0, []);

        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var skip     = (Math.Max(1, request.Page) - 1) * pageSize;

        var baseQuery = _context.RegistrosHoras
            .Where(r => r.UserId == userId
                && (request.Desde == null || r.FechaRegistro >= request.Desde)
                && (request.Hasta == null || r.FechaRegistro <= request.Hasta));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        // Se muestran los nombres guardados en el registro (foto del momento), no los
        // actuales del catalogo: el historial debe reflejar como se llamaban entonces.
        var items = await baseQuery
            .OrderByDescending(r => r.FechaRegistro)
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new MisRegistrosItemDto(
                r.Id,
                r.FechaRegistro,
                r.HoraEntrada1,
                r.HoraSalida1,
                r.HoraEntrada2,
                r.HoraSalida2,
                r.HoraEntrada3,
                r.HoraSalida3,
                r.ProyectoId,
                r.ClienteNombre,
                r.ProyectoNombre,
                r.Modalidad,
                r.Recurso,
                r.Descripcion,
                r.Lugar,
                r.Estado,
                r.ComentarioRechazo,
                Array.Empty<PasoAprobacionDto>()))
            .ToListAsync(cancellationToken);

        return new MisRegistrosPaginadosResponse(
            totalCount,
            await ConRastroDeAprobacionAsync(items, cancellationToken));
    }

    /// <summary>
    /// Adjunta a cada registro quien firmo cada nivel. Se resuelve en una consulta para
    /// toda la pagina, no una por fila; los nombres salen de Identity porque
    /// IApplicationDbContext no expone AspNetUsers.
    /// </summary>
    private async Task<List<MisRegistrosItemDto>> ConRastroDeAprobacionAsync(
        List<MisRegistrosItemDto> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0) return items;

        var ids = items.Select(i => i.Id).ToList();

        var pasos = await _context.AprobacionesRegistro
            .Where(a => ids.Contains(a.RegistroHorasId))
            .OrderBy(a => a.Created)
            .Select(a => new
            {
                a.RegistroHorasId,
                a.Nivel,
                a.Accion,
                a.ActorUserId,
                a.Created,
                a.Comentario
            })
            .ToListAsync(cancellationToken);

        if (pasos.Count == 0) return items;

        // El nombre, no el correo: "Laura Martinez" cabe en la celda y se entiende;
        // "gerente@kpg.com" desborda la columna y dice menos.
        var nombres = await _identity.GetUserNamesAsync(
            pasos.Select(p => p.ActorUserId).Distinct(), cancellationToken);

        var porRegistro = pasos
            .GroupBy(p => p.RegistroHorasId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<PasoAprobacionDto>)g.Select(p => new PasoAprobacionDto(
                        p.Nivel ?? 0,
                        p.Accion,
                        nombres.GetValueOrDefault(p.ActorUserId, p.ActorUserId),
                        p.Created,
                        p.Comentario)).ToList());

        return items
            .Select(i => porRegistro.TryGetValue(i.Id, out var rastro)
                ? i with { Aprobaciones = rastro }
                : i)
            .ToList();
    }
}
