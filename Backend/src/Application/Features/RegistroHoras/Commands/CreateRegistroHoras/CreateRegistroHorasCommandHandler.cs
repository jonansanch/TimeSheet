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

    public CreateRegistroHorasCommandHandler(IApplicationDbContext context, IUser user, IClock clock, IBitacoraService bitacora)
    {
        _context  = context;
        _user     = user;
        _clock    = clock;
        _bitacora = bitacora;
    }

    public async Task<RegistroHorasDto> Handle(CreateRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar el registro.");

        // Validar ventana de retroactividad
        var ventanaParam = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.VentanaRetroactividad, cancellationToken);
        var windowDays = ventanaParam != null && int.TryParse(ventanaParam.Valor, out var d) ? d : 3;

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

        // Intentar encontrar registro existente del mismo día y proyecto
        var existente = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.FechaRegistro == request.FechaRegistro &&
                r.Cliente == request.Cliente &&
                r.Proyecto == request.Proyecto,
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
                request.Cliente,
                request.Proyecto,
                request.Modalidad,
                request.Recurso,
                request.Descripcion,
                request.Lugar,
                esRetroactivo);

            _context.RegistrosHoras.Add(registro);
        }
        else
        {
            // Upsert: agregar los bloques horarios que faltaban
            if (request.HoraEntrada1.HasValue && request.HoraSalida1.HasValue)
            {
                existente.SetBloque(1, request.HoraEntrada1.Value, request.HoraSalida1.Value);
            }
            if (request.HoraEntrada2.HasValue && request.HoraSalida2.HasValue)
            {
                existente.SetBloque(2, request.HoraEntrada2.Value, request.HoraSalida2.Value);
            }
            if (request.HoraEntrada3.HasValue && request.HoraSalida3.HasValue)
            {
                existente.SetBloque(3, request.HoraEntrada3.Value, request.HoraSalida3.Value);
            }
            existente.UpdateMetadata(request.Cliente, request.Proyecto, request.Modalidad, request.Recurso, request.Lugar);
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
                registro.Cliente,
                registro.Proyecto
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
        r.Cliente, r.Proyecto, r.Modalidad, r.Recurso, r.Descripcion, r.Lugar,
        r.EsRetroactivo);
}
