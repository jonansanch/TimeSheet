using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Application.Features.Aprobaciones;

/// <summary>
/// Carga masiva de horas desde el Excel que usan los consultores.
///
/// <para>
/// <b>Se salta la ventana de retroactividad a proposito</b>: es una carga historica, no un
/// registro del dia a dia. Por eso esta restringida a Admin y queda en bitacora.
/// </para>
/// </summary>
[Authorize(Roles = Roles.Admin)]
public record ImportarTimesheetCommand(
    byte[] Contenido,
    string UserId,
    bool MarcarAprobado = true) : IRequest<ImportacionResultadoDto>;

public record ImportacionResultadoDto(
    string? NombreEnArchivo,
    int Mes,
    int Anio,
    int FilasLeidas,
    int Importadas,
    IReadOnlyList<FilaOmitidaDto> Omitidas,
    IReadOnlyList<FilaOmitidaDto> Errores,
    IReadOnlyList<FilaAdvertenciaDto> Advertencias)
{
    /// <summary>True si no se creo ningun registro: la pantalla lo destaca.</summary>
    public bool SinCambios => Importadas == 0;
}

public record FilaOmitidaDto(int NumeroFila, DateOnly? Fecha, string Detalle, string Motivo);

/// <summary>
/// Fila que si se importo, pero cuya descripcion tiene observaciones de calidad (ver
/// Docs/plan-calidad-descripciones.md). Es carga historica hecha por un Admin: no se
/// rechaza la fila, solo se informa para que se revise si hace falta.
/// </summary>
public record FilaAdvertenciaDto(int NumeroFila, DateOnly Fecha, string Detalle, IReadOnlyList<string> Avisos);

public class ImportarTimesheetCommandValidator : AbstractValidator<ImportarTimesheetCommand>
{
    public ImportarTimesheetCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Hay que indicar de quien son las horas.");
        RuleFor(x => x.Contenido)
            .Must(c => c is { Length: > 0 }).WithMessage("El archivo esta vacio.")
            .Must(c => c is null || c.Length <= 10 * 1024 * 1024)
            .WithMessage("El archivo no puede superar 10 MB.");
    }
}

public class ImportarTimesheetCommandHandler(
    IApplicationDbContext context,
    ITimesheetImportParser parser,
    IBitacoraService bitacora,
    IValidadorDescripcion validadorDescripcion,
    IUser actor)
    : IRequestHandler<ImportarTimesheetCommand, ImportacionResultadoDto>
{
    public async Task<ImportacionResultadoDto> Handle(
        ImportarTimesheetCommand request,
        CancellationToken cancellationToken)
    {
        var actorId = actor.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado.");

        using var stream = new MemoryStream(request.Contenido);
        var archivo = parser.Parse(stream);

        // Catalogo en memoria: el archivo trae nombres y el registro necesita ProyectoId.
        var proyectos = await (
            from p in context.Proyectos
            join c in context.Clientes on p.ClienteId equals c.Id
            where p.Activo && c.Activo
            select new { p.Id, Cliente = c.Nombre, Proyecto = p.Nombre })
            .ToListAsync(cancellationToken);

        var porNombre = proyectos.ToDictionary(
            x => Clave(x.Cliente, x.Proyecto),
            x => (x.Id, x.Cliente, x.Proyecto));

        // Lo ya registrado del mes, para no duplicar al reimportar el mismo archivo.
        var desde = new DateOnly(archivo.Anio!.Value, archivo.Mes!.Value, 1);
        var hasta = desde.AddMonths(1);
        var existentes = (await context.RegistrosHoras
            .Where(r => r.UserId == request.UserId && r.FechaRegistro >= desde && r.FechaRegistro < hasta)
            .Select(r => new { r.FechaRegistro, r.ProyectoId })
            .ToListAsync(cancellationToken))
            .Select(r => (r.FechaRegistro, r.ProyectoId))
            .ToHashSet();

        var omitidas    = new List<FilaOmitidaDto>();
        var advertencias = new List<FilaAdvertenciaDto>();
        var errores  = archivo.Errores
            .Select(e => new FilaOmitidaDto(e.NumeroFila, null, string.Empty, e.Motivo))
            .ToList();
        var nuevos = new List<RegistroHorasEntity>();

        foreach (var fila in archivo.Filas)
        {
            var detalle = $"{fila.Fecha:dd/MM} · {fila.Cliente} / {fila.Proyecto}";

            if (!porNombre.TryGetValue(Clave(fila.Cliente, fila.Proyecto), out var proyecto))
            {
                errores.Add(new FilaOmitidaDto(fila.NumeroFila, fila.Fecha, detalle,
                    "El cliente o el proyecto no existen en el catalogo, o estan inactivos."));
                continue;
            }

            if (!existentes.Add((fila.Fecha, proyecto.Id)))
            {
                omitidas.Add(new FilaOmitidaDto(fila.NumeroFila, fila.Fecha, detalle,
                    "Ya existe un registro para ese dia y proyecto."));
                continue;
            }

            try
            {
                var registro = new RegistroHorasEntity(
                    request.UserId,
                    fila.Fecha,
                    fila.HoraEntrada1, fila.HoraSalida1,
                    fila.HoraEntrada2, fila.HoraSalida2,
                    fila.HoraEntrada3, fila.HoraSalida3,
                    proyecto.Id,
                    proyecto.Cliente,
                    proyecto.Proyecto,
                    fila.Modalidad,
                    fila.Recurso,
                    fila.Descripcion,
                    fila.Lugar,
                    esRetroactivo: true);

                if (request.MarcarAprobado)
                {
                    // Salta la cadena entera: es una carga de horas ya validadas fuera del sistema.
                    registro.Aprobar(1);
                    registro.Aprobar(2);
                    registro.Aprobar(3);
                }

                // Es carga historica de un Admin: nunca se rechaza la fila, solo se avisa.
                var evaluacion = await validadorDescripcion.EvaluarAsync(
                    fila.Descripcion, proyecto.Id, cancellationToken);
                if (evaluacion.Hallazgos.Count > 0)
                {
                    advertencias.Add(new FilaAdvertenciaDto(
                        fila.NumeroFila, fila.Fecha, detalle,
                        evaluacion.Hallazgos.Select(h => h.Mensaje).ToList()));
                }

                nuevos.Add(registro);
            }
            catch (Domain.Exceptions.DomainRuleException ex)
            {
                errores.Add(new FilaOmitidaDto(fila.NumeroFila, fila.Fecha, detalle, ex.Message));
            }
        }

        if (nuevos.Count > 0)
        {
            context.RegistrosHoras.AddRange(nuevos);
            await context.SaveChangesAsync(cancellationToken);

            // El historial de aprobacion necesita el Id, que solo existe tras guardar.
            if (request.MarcarAprobado)
            {
                foreach (var registro in nuevos)
                    for (var nivel = 1; nivel <= 3; nivel++)
                        context.AprobacionesRegistro.Add(new AprobacionRegistro(
                            registro.Id, AprobacionRegistro.AccionAprobar, nivel, actorId,
                            "Aprobado automaticamente al importar el timesheet."));
            }

            await bitacora.RegistrarAsync(
                TipoEventoBitacora.ImportacionTimesheet,
                actorId, null,
                "RegistrosHoras", null,
                new
                {
                    EmpleadoUserId = request.UserId,
                    archivo.Mes,
                    archivo.Anio,
                    Importadas = nuevos.Count,
                    request.MarcarAprobado
                },
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        return new ImportacionResultadoDto(
            archivo.NombreConsultor,
            archivo.Mes!.Value,
            archivo.Anio!.Value,
            archivo.Filas.Count,
            nuevos.Count,
            omitidas,
            errores,
            advertencias);
    }

    /// <summary>Clave insensible a mayusculas y espacios sobrantes, como escriben los consultores.</summary>
    private static string Clave(string cliente, string proyecto) =>
        $"{cliente.Trim().ToLowerInvariant()}|{proyecto.Trim().ToLowerInvariant()}";
}
