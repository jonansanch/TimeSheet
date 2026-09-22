using System.Data;
using Dapper;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarReporteHoras;
using KPG.Timesheet.Application.Features.Reportes.Queries.ExportarTimesheet;
using MediatR;
using ParametrosSistemaKeys = KPG.Timesheet.Domain.Constants.ParametrosSistema;

namespace KPG.Timesheet.Infrastructure.Reportes;

/// <summary>
/// Resuelve los datos del timesheet mensual y delega el armado del documento en
/// <see cref="TimesheetDocumentBuilder"/>, que es donde vive el formato del cliente.
/// </summary>
public class ExportarTimesheetQueryHandler(IDbConnection db, IParametrosSistemaService parametros)
    : IRequestHandler<ExportarTimesheetQuery, ExportarTimesheetResult>
{
    private const string SqlNombre = """
        SELECT ISNULL(NombreCompleto, Email) FROM AspNetUsers WHERE Id = @UserId
        """;

    private const string Sql = """
        SELECT r.FechaRegistro,
               r.HoraEntrada1 AS Entrada1,
               r.HoraSalida1  AS Salida1,
               r.HoraEntrada2 AS Entrada2,
               r.HoraSalida2  AS Salida2,
               r.HoraEntrada3 AS Entrada3,
               r.HoraSalida3  AS Salida3,
               r.ClienteNombre  AS Cliente,
               r.ProyectoNombre AS Proyecto,
               r.Modalidad,
               r.Recurso,
               r.Lugar,
               r.Descripcion
        FROM   RegistrosHoras r
        WHERE  r.UserId = @UserId
          AND  r.FechaRegistro >= @Desde
          AND  r.FechaRegistro < @HastaExclusivo
          AND  (@Cliente  IS NULL OR r.ClienteNombre  = @Cliente)
          AND  (@Proyecto IS NULL OR r.ProyectoNombre = @Proyecto)
          AND  (@Recurso  IS NULL OR r.Recurso        = @Recurso)
        ORDER  BY r.FechaRegistro
        """;

    public async Task<ExportarTimesheetResult> Handle(
        ExportarTimesheetQuery request,
        CancellationToken cancellationToken)
    {
        var desde          = new DateOnly(request.Anio, request.Mes, 1);
        var hastaExclusivo = desde.AddMonths(1);

        var consultor = await db.ExecuteScalarAsync<string>(SqlNombre, new { request.UserId }) ?? request.UserId;

        var filas = (await db.QueryAsync<RawRow>(Sql, new
        {
            request.UserId,
            Desde          = desde,
            HastaExclusivo = hastaExclusivo,
            Cliente  = Vacio(request.Cliente),
            Proyecto = Vacio(request.Proyecto),
            Recurso  = Vacio(request.Recurso)
        })).Select(ToFila).ToList();

        var logo = await parametros.GetTextoAsync(ParametrosSistemaKeys.LogoReportes, string.Empty, cancellationToken);

        var esPdf = request.Formato == ExportFormato.Pdf;
        var contenido = esPdf
            ? TimesheetDocumentBuilder.Pdf(consultor, request.Mes, request.Anio, filas, logo)
            : TimesheetDocumentBuilder.Excel(consultor, request.Mes, request.Anio, filas, logo);

        var mes     = TimesheetDocumentBuilder.NombreDelMes(request.Mes, request.Anio).ToLowerInvariant();
        var archivo = $"timesheet-{consultor.Replace(" ", "-").ToLowerInvariant()}-" +
                      $"{mes}-{request.Anio}.{(esPdf ? "pdf" : "xlsx")}";

        return new ExportarTimesheetResult(
            contenido,
            esPdf ? "application/pdf" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            archivo);
    }

    private static TimesheetFila ToFila(RawRow r) => new(
        DateOnly.FromDateTime(r.FechaRegistro),
        ToTime(r.Entrada1), ToTime(r.Salida1),
        ToTime(r.Entrada2), ToTime(r.Salida2),
        ToTime(r.Entrada3), ToTime(r.Salida3),
        r.Cliente, r.Proyecto, r.Modalidad, r.Recurso,
        r.Lugar ?? string.Empty, r.Descripcion ?? string.Empty);

    private static TimeOnly? ToTime(TimeSpan? valor) =>
        valor.HasValue ? TimeOnly.FromTimeSpan(valor.Value) : null;

    private static string? Vacio(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private sealed record RawRow(
        DateTime  FechaRegistro,
        TimeSpan? Entrada1,
        TimeSpan? Salida1,
        TimeSpan? Entrada2,
        TimeSpan? Salida2,
        TimeSpan? Entrada3,
        TimeSpan? Salida3,
        string    Cliente,
        string    Proyecto,
        string    Modalidad,
        string    Recurso,
        string?   Lugar,
        string?   Descripcion);
}
