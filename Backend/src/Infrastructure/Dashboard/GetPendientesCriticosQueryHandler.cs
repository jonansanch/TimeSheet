using System.Data;
using Dapper;
using KPG.Timesheet.Application.Features.Dashboard.Queries.GetPendientesCriticos;
using MediatR;

namespace KPG.Timesheet.Infrastructure.Dashboard;

public class GetPendientesCriticosQueryHandler(IDbConnection db)
    : IRequestHandler<GetPendientesCriticosQuery, PendientesCriticosResponse>
{
    private const string SqlUmbral = """
        SELECT ISNULL(TRY_CAST(Valor AS int), 3)
        FROM   ParametrosSistema
        WHERE  Clave = @Clave
        """;

    private const string SqlUsuarios = """
        SELECT u.Id                               AS UserId,
               ISNULL(u.NombreCompleto, u.Email)  AS Nombre,
               u.Email,
               MAX(r.FechaRegistro)               AS UltimoRegistro
        FROM   AspNetUsers u
        JOIN   AspNetUserRoles ur ON u.Id = ur.UserId
        JOIN   AspNetRoles ro     ON ur.RoleId = ro.Id
        LEFT   JOIN RegistrosHoras r ON r.UserId = u.Id
        WHERE  u.IsActive = 1
          AND  ro.Name IN ('Empleado', 'Supervisor')
        GROUP  BY u.Id, u.NombreCompleto, u.Email
        HAVING MAX(r.FechaRegistro) < @FechaCorte
            OR MAX(r.FechaRegistro) IS NULL
        ORDER  BY MAX(r.FechaRegistro) ASC
        """;

    public async Task<PendientesCriticosResponse> Handle(
        GetPendientesCriticosQuery request,
        CancellationToken cancellationToken)
    {
        var umbral = await db.ExecuteScalarAsync<int>(SqlUmbral,
            new { Clave = Domain.Constants.ParametrosSistema.DiasUmbralNotificacion });
        if (umbral <= 0) umbral = 3;

        var fechaCorte = RestarDiasHabiles(DateOnly.FromDateTime(DateTime.Today), umbral);

        var rows = await db.QueryAsync<RawRow>(SqlUsuarios, new { FechaCorte = fechaCorte });

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var pendientes = rows.Select(r => new PendienteCriticoDto(
            r.UserId,
            r.Nombre,
            r.Email,
            r.UltimoRegistro.HasValue
                ? ContarDiasHabiles(r.UltimoRegistro.Value, hoy)
                : 999)).ToList();

        return new PendientesCriticosResponse(umbral, pendientes);
    }

    private static DateOnly RestarDiasHabiles(DateOnly fecha, int dias)
    {
        int restados = 0;
        while (restados < dias)
        {
            fecha = fecha.AddDays(-1);
            if (fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                restados++;
        }
        return fecha;
    }

    private static int ContarDiasHabiles(DateOnly desde, DateOnly hasta)
    {
        int count = 0;
        var d = desde.AddDays(1);
        while (d <= hasta)
        {
            if (d.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
                count++;
            d = d.AddDays(1);
        }
        return Math.Min(count, 999);
    }

    private sealed record RawRow(string UserId, string Nombre, string Email, DateOnly? UltimoRegistro);
}
