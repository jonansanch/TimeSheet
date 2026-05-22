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

        // Intentar encontrar registro existente del día
        var existente = await _context.RegistrosHoras
            .FirstOrDefaultAsync(r => r.UserId == userId && r.FechaRegistro == request.FechaRegistro, cancellationToken);

        RegistroHorasEntity registro;

        if (existente is null)
        {
            // Crear nuevo registro diario
            registro = new RegistroHorasEntity(
                userId,
                request.FechaRegistro,
                request.HoraEntradaAM,
                request.HoraSalidaAM,
                request.HoraEntradaPM,
                request.HoraSalidaPM,
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
            // Upsert: agregar el bloque que faltaba
            if (request.HoraEntradaAM.HasValue && request.HoraSalidaAM.HasValue)
            {
                existente.SetBloqueAM(request.HoraEntradaAM.Value, request.HoraSalidaAM.Value);
            }
            if (request.HoraEntradaPM.HasValue && request.HoraSalidaPM.HasValue)
            {
                existente.SetBloquePM(request.HoraEntradaPM.Value, request.HoraSalidaPM.Value);
            }
            existente.UpdateMetadata(request.Cliente, request.Proyecto, request.Modalidad, request.Recurso, request.Lugar);
            existente.UpdateDescripcion(request.Descripcion);
            registro = existente;
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.RegistroHorasCreado,
            userId, null,
            "RegistrosHoras", null,
            new { registro.FechaRegistro, TieneAM = registro.TieneAM, TienePM = registro.TienePM, registro.Cliente, registro.Proyecto },
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return ToDto(registro);
    }

    private static RegistroHorasDto ToDto(RegistroHorasEntity r) => new(
        r.Id, r.UserId, r.FechaRegistro,
        r.HoraEntradaAM, r.HoraSalidaAM,
        r.HoraEntradaPM, r.HoraSalidaPM,
        r.Cliente, r.Proyecto, r.Modalidad, r.Recurso, r.Descripcion, r.Lugar,
        r.EsRetroactivo);
}
