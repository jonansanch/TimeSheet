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
        _context = context;
        _user = user;
        _clock = clock;
        _bitacora = bitacora;
    }

    public async Task<RegistroHorasDto> Handle(CreateRegistroHorasCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar el registro.");

        var ventanaParam = await _context.ParametrosSistema
            .FirstOrDefaultAsync(p => p.Clave == Domain.Constants.ParametrosSistema.VentanaRetroactividad, cancellationToken);
        var windowDays = ventanaParam != null && int.TryParse(ventanaParam.Valor, out var d) ? d : 3;

        var today = _clock.Today;
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
            {
                throw new ApplicationValidationException([
                    new ValidationFailure(
                        nameof(request.FechaRegistro),
                        $"La fecha seleccionada está fuera de la ventana de registro permitida ({windowDays} días hábiles).")
                ]);
            }
        }

        var esRetroactivo = request.FechaRegistro < today;

        var registro = new RegistroHorasEntity(
            userId,
            request.FechaRegistro,
            request.Turno,
            request.HoraEntrada,
            request.HoraSalida,
            request.Cliente,
            request.Proyecto,
            request.Modalidad,
            request.Recurso,
            request.Descripcion,
            request.Lugar,
            esRetroactivo);

        _context.RegistrosHoras.Add(registro);
        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.RegistroHorasCreado,
            userId, null,
            "RegistrosHoras", null,
            new { registro.FechaRegistro, Turno = registro.Turno.ToString(), registro.Cliente, registro.Proyecto },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new RegistroHorasDto(
            registro.Id,
            registro.UserId,
            registro.FechaRegistro,
            registro.Turno,
            registro.HoraEntrada,
            registro.HoraSalida,
            registro.Cliente,
            registro.Proyecto,
            registro.Modalidad,
            registro.Recurso,
            registro.Descripcion,
            registro.Lugar,
            registro.EsRetroactivo);
    }
}
