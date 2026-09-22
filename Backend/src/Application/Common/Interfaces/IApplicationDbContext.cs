using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<RegistroHoras> RegistrosHoras { get; }
    DbSet<ParametroSistema> ParametrosSistema { get; }
    DbSet<SolicitudExcepcion> SolicitudesExcepcion { get; }
    DbSet<Empleado> Empleados { get; }
    DbSet<Cliente> Clientes { get; }
    DbSet<Proyecto> Proyectos { get; }
    DbSet<Modalidad> Modalidades { get; }
    DbSet<LugarTrabajo> LugaresTrabajo { get; }
    DbSet<NotificacionEnviada> NotificacionesEnviadas { get; }
    DbSet<BitacoraAuditoria> BitacoraAuditoria { get; }
    DbSet<SupervisorPuesto> SupervisoresPuesto { get; }
    DbSet<ReglaVentanaRetroactividad> ReglasVentanaRetroactividad { get; }
    DbSet<ParametroRestriccionDia> ParametrosRestriccionDia { get; }
    DbSet<AprobacionRegistro> AprobacionesRegistro { get; }
    DbSet<ReporteUsuario> ReportesUsuario { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
