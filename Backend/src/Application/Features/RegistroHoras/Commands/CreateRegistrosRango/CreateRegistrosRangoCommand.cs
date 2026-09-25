using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Application.Common.Security;
using KPG.Timesheet.Application.Common.Validation;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistrosRango;

/// <summary>
/// Crea el mismo registro para cada dia habil de un rango. Pensado para jornadas
/// repetidas: una semana entera en el mismo proyecto con el mismo horario.
///
/// <para>
/// Los sabados y domingos se saltan. Es una carga masiva a proposito: no pasa por la
/// ventana de retroactividad, ni por restricciones de dia, ni evita duplicados, y queda
/// aprobada en los 3 niveles de una vez — como la importacion de Excel.
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

public record RegistrosRangoResultadoDto(int Creados)
{
    public bool SinCambios => Creados == 0;
}

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

/// <summary>
/// Calidad de la descripcion contra el catalogo de terminos, aparte del validador de forma
/// de arriba por la misma razon que en <c>CreateRegistroHoras</c>: necesita base de datos.
/// </summary>
public class CreateRegistrosRangoDescripcionValidator : AbstractValidator<CreateRegistrosRangoCommand>
{
    public CreateRegistrosRangoDescripcionValidator(IValidadorDescripcion validador)
    {
        RuleFor(x => x.Descripcion)
            .DescripcionValida(validador, (cmd, _) => Task.FromResult<int?>(cmd.ProyectoId));
    }
}

public class CreateRegistrosRangoCommandHandler(
    IApplicationDbContext context,
    IUser user,
    IClock clock,
    IBitacoraService bitacora)
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

        var hoy    = clock.Today;
        var nuevos = new List<RegistroHorasEntity>();

        for (var fecha = request.Desde; fecha <= request.Hasta; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == DayOfWeek.Sunday ||
               (fecha.DayOfWeek == DayOfWeek.Saturday && !request.IncluirSabados))
                continue;   // no laborable: se salta en silencio, no es un problema

            var registro = new RegistroHorasEntity(
                userId, fecha,
                request.HoraEntrada1, request.HoraSalida1,
                request.HoraEntrada2, request.HoraSalida2,
                request.HoraEntrada3, request.HoraSalida3,
                request.ProyectoId, proyecto.Cliente, proyecto.Proyecto,
                request.Modalidad, request.Recurso, request.Descripcion, request.Lugar,
                esRetroactivo: fecha < hoy);

            // Carga masiva ya validada por quien la registra: se aprueba en los 3 niveles
            // de una vez, igual que la importacion de Excel.
            registro.Aprobar(1);
            registro.Aprobar(2);
            registro.Aprobar(3);

            nuevos.Add(registro);
        }

        if (nuevos.Count > 0)
        {
            context.RegistrosHoras.AddRange(nuevos);
            await context.SaveChangesAsync(cancellationToken);

            // El historial de aprobacion necesita el Id, que solo existe tras guardar.
            foreach (var registro in nuevos)
                for (var nivel = 1; nivel <= 3; nivel++)
                    context.AprobacionesRegistro.Add(new AprobacionRegistro(
                        registro.Id, AprobacionRegistro.AccionAprobar, nivel, userId,
                        "Aprobado automaticamente al registrar por rango de fechas."));

            await bitacora.RegistrarAsync(
                TipoEventoBitacora.RegistroHorasCreado,
                userId, null,
                "RegistrosHoras", null,
                new { request.Desde, request.Hasta, request.ProyectoId, Creados = nuevos.Count },
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        return new RegistrosRangoResultadoDto(nuevos.Count);
    }
}
