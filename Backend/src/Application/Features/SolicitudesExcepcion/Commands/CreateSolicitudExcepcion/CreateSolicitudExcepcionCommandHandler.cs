using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Constants;
using KPG.Timesheet.Domain.Entities;

namespace KPG.Timesheet.Application.Features.SolicitudesExcepcion.Commands.CreateSolicitudExcepcion;

public class CreateSolicitudExcepcionCommandHandler : IRequestHandler<CreateSolicitudExcepcionCommand, SolicitudExcepcionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IBitacoraService _bitacora;

    public CreateSolicitudExcepcionCommandHandler(IApplicationDbContext context, IUser user, IBitacoraService bitacora)
    {
        _context = context;
        _user = user;
        _bitacora = bitacora;
    }

    public async Task<SolicitudExcepcionDto> Handle(CreateSolicitudExcepcionCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("No existe usuario autenticado para asociar la solicitud.");

        var solicitud = new SolicitudExcepcion(userId, request.FechaRegistro, request.Justificacion);

        if (request.TraeRegistro)
        {
            // Se guarda la foto de los nombres igual que en un registro normal: si el
            // proyecto se renombra antes de aprobar, el registro conservara como se llamaba.
            var proyecto = await _context.Proyectos
                .Where(p => p.Id == request.ProyectoId && p.Activo)
                .Join(_context.Clientes.Where(c => c.Activo), p => p.ClienteId, c => c.Id,
                      (p, c) => new { Cliente = c.Nombre, Proyecto = p.Nombre })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new Common.Exceptions.ValidationException([
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.ProyectoId),
                        "El proyecto seleccionado no existe, esta inactivo o su cliente esta inactivo.")]);

            solicitud.AdjuntarRegistro(
                request.ProyectoId!.Value, proyecto.Cliente, proyecto.Proyecto,
                request.HoraEntrada1, request.HoraSalida1,
                request.HoraEntrada2, request.HoraSalida2,
                request.HoraEntrada3, request.HoraSalida3,
                request.Modalidad!, request.Recurso!, request.Lugar!, request.Descripcion!);
        }

        _context.SolicitudesExcepcion.Add(solicitud);
        await _bitacora.RegistrarAsync(
            TipoEventoBitacora.SolicitudExcepcionCreada,
            userId, null,
            "SolicitudesExcepcion", null,
            new { solicitud.FechaRegistro },
            cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new SolicitudExcepcionDto(
            solicitud.Id,
            solicitud.UserId,
            solicitud.FechaRegistro,
            solicitud.Justificacion,
            solicitud.Estado.ToString());
    }
}
