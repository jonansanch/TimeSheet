using System.Reflection;
using KPG.Timesheet.Application.Common.Interfaces;
using KPG.Timesheet.Domain.Entities;
using KPG.Timesheet.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KPG.Timesheet.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RegistroHoras> RegistrosHoras => Set<RegistroHoras>();
    public DbSet<ParametroSistema> ParametrosSistema => Set<ParametroSistema>();
    public DbSet<SolicitudExcepcion> SolicitudesExcepcion => Set<SolicitudExcepcion>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Modalidad> Modalidades => Set<Modalidad>();
    public DbSet<LugarTrabajo> LugaresTrabajo => Set<LugarTrabajo>();
    public DbSet<NotificacionEnviada> NotificacionesEnviadas => Set<NotificacionEnviada>();
    public DbSet<BitacoraAuditoria> BitacoraAuditoria => Set<BitacoraAuditoria>();
    public DbSet<SupervisorPuesto> SupervisoresPuesto => Set<SupervisorPuesto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.IsActive)
            .HasDatabaseName("IX_AspNetUsers_IsActive");

        // Auto-referencia: el organigrama es una jerarquia de usuarios. Restrict evita
        // que borrar a un jefe arrastre a su equipo.
        builder.Entity<ApplicationUser>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(u => u.SupervisorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.SupervisorUserId)
            .HasDatabaseName("IX_AspNetUsers_SupervisorUserId");

        builder.Entity<ApplicationUser>()
            .HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(u => u.PuestoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
