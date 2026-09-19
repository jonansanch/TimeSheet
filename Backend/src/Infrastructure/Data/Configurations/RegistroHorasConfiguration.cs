using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class RegistroHorasConfiguration : IEntityTypeConfiguration<RegistroHoras>
{
    public void Configure(EntityTypeBuilder<RegistroHoras> builder)
    {
        builder.ToTable("RegistrosHoras");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).HasMaxLength(450).IsRequired();
        builder.Property(r => r.FechaRegistro).IsRequired();

        builder.Property(r => r.HoraEntrada1).IsRequired(false);
        builder.Property(r => r.HoraSalida1).IsRequired(false);
        builder.Property(r => r.HoraEntrada2).IsRequired(false);
        builder.Property(r => r.HoraSalida2).IsRequired(false);
        builder.Property(r => r.HoraEntrada3).IsRequired(false);
        builder.Property(r => r.HoraSalida3).IsRequired(false);

        builder.Property(r => r.ProyectoId).IsRequired();
        builder.Property(r => r.ClienteNombre).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ProyectoNombre).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Modalidad).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Recurso).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Lugar).HasMaxLength(200).IsRequired();
        builder.Property(r => r.EsRetroactivo).IsRequired().HasDefaultValue(false);

        builder.Property(r => r.Estado).IsRequired().HasDefaultValue(Domain.Enums.EstadoAprobacion.Pendiente);
        builder.Property(r => r.EstadoPrevioAlRechazo);
        builder.Property(r => r.ComentarioRechazo).HasMaxLength(1000);

        // La pantalla de revision filtra por estado dentro de un rango de fechas.
        builder.HasIndex(r => new { r.Estado, r.FechaRegistro })
            .HasDatabaseName("IX_RegistrosHoras_Estado_FechaRegistro");

        builder.Ignore(r => r.TieneHorario1);
        builder.Ignore(r => r.TieneHorario2);
        builder.Ignore(r => r.TieneHorario3);
        builder.Ignore(r => r.TotalMinutos);
        builder.Ignore(r => r.EstaAprobado);
        builder.Ignore(r => r.EstaRechazado);
        builder.Ignore(r => r.NivelPendiente);

        // Restrict: un proyecto con horas imputadas no se puede borrar.
        builder.HasOne<Proyecto>()
            .WithMany()
            .HasForeignKey(r => r.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un registro por usuario/día/proyecto (puede haber varios proyectos el mismo día)
        builder.HasIndex(r => new { r.UserId, r.FechaRegistro, r.ProyectoId })
            .HasDatabaseName("IX_RegistrosHoras_UserId_FechaRegistro_ProyectoId")
            .IsUnique();

        builder.HasIndex(r => r.FechaRegistro)
            .HasDatabaseName("IX_RegistrosHoras_FechaRegistro")
            .IncludeProperties(r => r.UserId);
    }
}
