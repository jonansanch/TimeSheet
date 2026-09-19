using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistrosRango;

/// <summary>
/// Crea el mismo registro para cada dia habil de un rango. Pensado para jornadas
/// repetidas: una semana entera en el mismo proyecto con el mismo horario.
///
/// <para>
/// Los sabados y domingos se saltan, y cada dia se valida por separado contra la ventana
/// de retroactividad: el rango no es una via para saltarsela.
/// </para>
/// </summary>
[Authorize(Roles = $"{Roles.Empleado},{Roles.Supervisor},{Roles.Gerente},{Roles.Admin}")]
public record CreateRegistrosRangoCommand(
    DateOnly  Desde,
    DateOnly  Hasta,
    TimeOnly? HoraEntrada1,
    TimeOnly? HoraSalida1,
    TimeOnly? HoraEntrada2,
    TimeOnly? HoraSalida2,
    TimeOnly? HoraEntrada3,
    TimeOnly? HoraSalida3,
    int       ProyectoId,
    string    Modalidad,
    string    Recurso,
    string    Descripcion,
    string    Lugar,
    bool      IncluirSabados = false) : IRequest<RegistrosRangoResultadoDto>;

public record RegistrosRangoResultadoDto(
    int Creados,
    IReadOnlyList<DiaOmitidoDto> Omitidos)
{
    public bool SinCambios => Creados == 0;
}

public record DiaOmitidoDto(DateOnly Fecha, string Motivo);

public class CreateRegistrosRangoCommandValidator : AbstractValidator<CreateRegistrosRangoCommand>
{
    /// <summary>Tope para que un rango mal tecleado no genere cientos de registros.</summary>
    public const int MaxDias = 62;

    public CreateRegistrosRangoCommandValidator()
    {
        RuleFor(x => x.Hasta)
            .GreaterThanOrEqualTo(x => x.Desde)
            .WithMessage("La fecha final no puede ser anterior a la inicial.");

        RuleFor(x => x)
            .Must(x => x.Hasta.DayNumber - x.Desde.DayNumber < MaxDias)
            .WithMessage($"El rango no puede superar {MaxDias} dias.");

        RuleFor(x => x)
            .Must(x => x.HoraEntrada1.HasValue || x.HoraEntrada2.HasValue || x.HoraEntrada3.HasValue)
            .WithMessage("Debe registrar al menos un horario.");

        RuleFor(x => x.ProyectoId).GreaterThan(0).WithMessage("El proyecto es requerido.");
        RuleFor(x => x.Modalidad).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Recurso).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Lugar).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Descripcion).NotEmpty().MaximumLength(1000);
    }
}

public class CreateRegistrosRangoCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IClock clock,
    IBitacoraService bitacora,
    IVentanaRetroactividadService ventana)
    : IRequestHandler<CreateRegistrosRangoCommand, RegistrosRangoResultadoDto>
{
    public async Task<RegistrosRangoResultadoDto> Handle(
        CreateRegistrosRangoCommand request,
        CancellationToken cancellationToken)
    {
        var userId = user.Id
            ?? throw new UnauthorizedAccessException("No existe usuario autenticado para asociar el registro.");

        var proyecto = await context.Proyectos
            .Where(p => p.Id == request.ProyectoId && p.Activo)
            .Join(context.Clientes.Where(c => c.Activo), p => p.ClienteId, c => c.Id,
                  (p, c) => new { Cliente = c.Nombre, Proyecto = p.Nombre })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new Common.Exceptions.ValidationException([
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ProyectoId),
                    "El proyecto seleccionado no existe, esta inactivo o su cliente esta inactivo.")]);

        var windowDays      = await ventana.GetDiasAsync(userId, user.Roles, cancellationToken);
        var hoy             = clock.Today;
        var fechaMasAntigua = BusinessDayCalculator.GetEarliestAllowedDate(hoy, windowDays);

        var existentes = (await context.RegistrosHoras
            .Where(r => r.UserId == userId
                     && r.ProyectoId == request.ProyectoId
                     && r.FechaRegistro >= request.Desde
                     && r.FechaRegistro <= request.Hasta)
            .Select(r => r.FechaRegistro)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var excepciones = (await context.SolicitudesExcepcion
            .Where(s => s.UserId == userId
                     && s.Estado == EstadoSolicitud.Aprobada
                     && s.FechaRegistro >= request.Desde
                     && s.FechaRegistro <= request.Hasta)
            .Select(s => s.FechaRegistro)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var omitidos = new List<DiaOmitidoDto>();
        var nuevos   = new List<RegistroHorasEntity>();

        for (var fecha = request.Desde; fecha <= request.Hasta; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == DayOfWeek.Sunday ||
               (fecha.DayOfWeek == DayOfWeek.Saturday && !request.IncluirSabados))
                continue;   // no laborable: se salta en silencio, no es un problema

            if (fecha > hoy)
            {
                omitidos.Add(new DiaOmitidoDto(fecha, "Es una fecha futura."));
                continue;
            }

            if (existentes.Contains(fecha))
            {
                omitidos.Add(new DiaOmitidoDto(fecha, "Ya tienes un registro de ese dia en este proyecto."));
                continue;
            }

            if (fecha < fechaMasAntigua && !excepciones.Contains(fecha))
            {
                omitidos.Add(new DiaOmitidoDto(fecha,
                    $"Esta fuera de la ventana de registro ({windowDays} dias habiles) y no tiene excepcion aprobada."));
                continue;
            }

            nuevos.Add(new RegistroHorasEntity(
                userId, fecha,
                request.HoraEntrada1, request.HoraSalida1,
                request.HoraEntrada2, request.HoraSalida2,
                request.HoraEntrada3, request.HoraSalida3,
                request.ProyectoId, proyecto.Cliente, proyecto.Proyecto,
                request.Modalidad, request.Recurso, request.Descripcion, request.Lugar,
                esRetroactivo: fecha < hoy));
        }

        if (nuevos.Count > 0)
        {
            context.RegistrosHoras.AddRange(nuevos);

            await bitacora.RegistrarAsync(
                TipoEventoBitacora.RegistroHorasCreado,
                userId, null,
                "RegistrosHoras", null,
                new { request.Desde, request.Hasta, request.ProyectoId, Creados = nuevos.Count },
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        return new RegistrosRangoResultadoDto(nuevos.Count, omitidos);
    }
}
