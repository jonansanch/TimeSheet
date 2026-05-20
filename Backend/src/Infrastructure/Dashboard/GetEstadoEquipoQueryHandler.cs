using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetEstadoEquipo;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class GetEstadoEquipoQueryHandler(IDbConnection db)
    : IRequestHandler<GetEstadoEquipoQuery, EstadoEquipoResponse>
{
    private const string Sql = """
        SELECT u.Id                               AS UserId,
               ISNULL(u.NombreCompleto, u.Email) AS Nombre,
               u.Email,
               MAX(CASE WHEN r.Turno = 1 THEN 1 ELSE 0 END) AS TieneAM,
               MAX(CASE WHEN r.Turno = 2 THEN 1 ELSE 0 END) AS TienePM
        FROM   AspNetUsers u
        JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
        JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
        LEFT   JOIN RegistrosHoras r
               ON r.UserId = u.Id AND r.FechaRegistro = @Fecha
        WHERE  u.IsActive = 1
          AND  ro.Name IN ('Empleado', 'Supervisor')
        GROUP  BY u.Id, u.NombreCompleto, u.Email
        ORDER  BY ISNULL(u.NombreCompleto, u.Email)
        """;

    public async Task<EstadoEquipoResponse> Handle(
        GetEstadoEquipoQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await db.QueryAsync<RawRow>(Sql, new { Fecha = request.Fecha });

        var equipo = rows.Select(r =>
        {
            var tieneAm = r.TieneAM == 1;
            var tienePm = r.TienePM == 1;
            var estado = (tieneAm, tienePm) switch
            {
                (true, true)   => "Completo",
                (false, false) => "Pendiente",
                _              => "Parcial"
            };
            return new MiembroEstadoDto(r.UserId, r.Nombre, r.Email, tieneAm, tienePm, estado);
        }).ToList();

        return new EstadoEquipoResponse(
            Fecha:           request.Fecha,
            TotalEmpleados:  equipo.Count,
            TotalCompletos:  equipo.Count(m => m.Estado == "Completo"),
            TotalParciales:  equipo.Count(m => m.Estado == "Parcial"),
            TotalPendientes: equipo.Count(m => m.Estado == "Pendiente"),
            Equipo:          equipo);
    }

    private sealed record RawRow(string UserId, string Nombre, string Email, int TieneAM, int TienePM);
}
