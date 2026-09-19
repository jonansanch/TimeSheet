using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;
using NotFoundException = KPG.Timesheet.Application.Common.Exceptions.NotFoundException;
using RegistroHorasEntity = KPG.Timesheet.Domain.Entities.RegistroHoras;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.AprobarSolicitudExcepcion;

public class AprobarSolicitudExcepcionCommandHandler : IRequestHandler<AprobarSolicitudExcepcionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IBitacoraService _bitacora;
    private readonly IUser _user;

    public AprobarSolicitudExcepcionCommandHandler(IApplicationDbContext context, IBitacoraService bitacora, IUser user)
    {
        _context = context;
        _bitacora = bitacora;
        _user = user;
    }

    public async Task Handle(AprobarSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var solicitud = await _context.SolicitudesExcepcion
            .FindAsync([request.Id], cancellationToken);

        if (solicitud is null)
            throw new NotFoundException($"SolicitudExcepcion con Id {request.Id} no encontrada.");

        solicitud.Aprobar();

        // Con registro adjunto el empleado no tiene que volver a llenar nada: se crea aqui.
        // Si ya existe uno de ese dia y proyecto se deja como esta, para no duplicar.
        if (solicitud.TieneRegistro)
        {
            var yaExiste = await _context.RegistrosHoras.AnyAsync(
                r => r.UserId == solicitud.UserId
                  && r.FechaRegistro == solicitud.FechaRegistro
                  && r.ProyectoId == solicitud.ProyectoId,
                cancellationToken);

            if (!yaExiste)
            {
                _context.RegistrosHoras.Add(new RegistroHorasEntity(
                    solicitud.UserId,
                    solicitud.FechaRegistro,
                    solicitud.HoraEntrada1, solicitud.HoraSalida1,
                    solicitud.HoraEntrada2, solicitud.HoraSalida2,
                    solicitud.HoraEntrada3, solicitud.HoraSalida3,
                    solicitud.ProyectoId!.Value,
                    solicitud.ClienteNombre!,
                    solicitud.ProyectoNombre!,
                    solicitud.Modalidad!,
                    solicitud.Recurso!,
                    solicitud.Descripcion!,
                    solicitud.Lugar!,
                    esRetroactivo: true));
            }
        }

        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.AprobacionExcepcion,
            _user.Id ?? "system", null,
            "SolicitudesExcepcion", request.Id.ToString(),
            new { solicitud.UserId, Fecha = solicitud.FechaRegistro },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
