using FluentValidation.Results;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Common;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;
using ApplicationValidationException = KPG.Timesheet.Application.Common.Exceptions.ValidationException;

namespace KPG.Timesheet.Application.Features.RegistroHoras.Commands.CreateRegistroHoras;

public class CreateRegistroHorasCommandHandler : IRequestHandler<CreateRegistroHorasCommand, RegistroHorasDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IClock _clock;
    private readonly IBitacoraService _bitacora;
    private readonly IVentanaRetroactividadService _ventana;

    public CreateRegistroHorasCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IClock clock,
        IBitacoraService bitacora,
        IVentanaRetroactividadService ventana)
    {
        _context  = context;
        _user     = user;
        _clock    = clock;
        _bitacora = bitacora;
        _ventana  = ventana;
    }

    public async Task<RegistroHorasDto> Handle(CreateRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar el registro.");

        // Ventana de retroactividad efectiva: puede tener excepcion por persona o por rol.
        var windowDays = await _ventana.GetDiasAsync(userId, _user.Roles, cancellationToken);

        var today          = _clock.Today;
        var earliestAllowed = BusinessDayCalculator.GetEarliestAllowedDate(today, windowDays);

        if (request.FechaRegistro < earliestAllowed)
        {
            var tieneExcepcionAprobada = await _context.SolicitudesExcepcion
                .AnyAsync(s =>
                    s.UserId == userId &&
                    s.FechaRegistro == request.FechaRegistro &&
                    s.Estado == EstadoSolicitud.Aprobada,
                    cancellationToken);

            if (!tieneExcepcionAprobada)
                throw new ApplicationValidationException([
                    new ValidationFailure(
                        nameof(request.FechaRegistro),
                        $"La fecha seleccionada está fuera de la ventana de registro permitida ({windowDays} días hábiles).")
                ]);
        }

        var esRetroactivo = request.FechaRegistro < today;

        // Foto de los nombres al momento de registrar: el registro debe conservar como se
        // llamaban cliente y proyecto aunque luego se renombren en el catalogo.
        var nombres = await _context.Proyectos
            .Where(p => p.Id == request.ProyectoId)
            .Join(_context.Clientes, p => p.ClienteId, c => c.Id,
                  (p, c) => new { Cliente = c.Nombre, Proyecto = p.Nombre })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ApplicationValidationException([
                new ValidationFailure(nameof(request.ProyectoId), "El proyecto seleccionado no existe.")]);

        // Intentar encontrar registro existente del mismo día y proyecto
        var existente = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.FechaRegistro == request.FechaRegistro &&
                r.ProyectoId == request.ProyectoId,
                cancellationToken);

        RegistroHorasEntity registro;

        if (existente is null)
        {
            // Crear nuevo registro diario
            registro = new RegistroHorasEntity(
                userId,
                request.FechaRegistro,
                request.HoraEntrada1,
                request.HoraSalida1,
                request.HoraEntrada2,
                request.HoraSalida2,
                request.HoraEntrada3,
                request.HoraSalida3,
                request.ProyectoId,
                nombres.Cliente,
                nombres.Proyecto,
                request.Modalidad,
                request.Recurso,
                request.Descripcion,
                request.Lugar,
                esRetroactivo);

            _context.RegistrosHoras.Add(registro);
        }
        else
        {
            // Upsert: los horarios enviados se suman a la jornada del dia, cada uno en el
            // primer bloque libre. La casilla que uso el formulario no importa: lo unico
            // que se exige es que no se crucen con lo ya registrado.
            foreach (var (entrada, salida) in new[]
            {
                (request.HoraEntrada1, request.HoraSalida1),
                (request.HoraEntrada2, request.HoraSalida2),
                (request.HoraEntrada3, request.HoraSalida3)
            })
            {
                if (entrada.HasValue && salida.HasValue)
                    existente.AgregarHorario(entrada.Value, salida.Value);
            }

            existente.UpdateMetadata(request.Modalidad, request.Recurso, request.Lugar);
            existente.UpdateDescripcion(request.Descripcion);
            registro = existente;
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.RegistroHorasCreado,
            userId, null,
            "RegistrosHoras", null,
            new
            {
                registro.FechaRegistro,
                registro.TieneHorario1,
                registro.TieneHorario2,
                registro.TieneHorario3,
                registro.ProyectoId
            },
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(registro);
    }

    private static RegistroHorasDto ToDto(RegistroHorasEntity r) => new(
        r.Id, r.UserId, r.FechaRegistro,
        r.HoraEntrada1, r.HoraSalida1,
        r.HoraEntrada2, r.HoraSalida2,
        r.HoraEntrada3, r.HoraSalida3,
        r.ProyectoId, r.ClienteNombre, r.ProyectoNombre,
        r.Modalidad, r.Recurso, r.Descripcion, r.Lugar,
        r.EsRetroactivo);
}
