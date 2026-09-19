using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class SolicitudExcepcionConfiguration : IEntityTypeConfiguration<SolicitudExcepcion>
{
    public void Configure(EntityTypeBuilder<SolicitudExcepcion> builder)
    {
        builder.ToTable("SolicitudesExcepcion");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.FechaRegistro).IsRequired();
        builder.Property(s => s.Justificacion).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Registro adjunto: opcional, para no invalidar las solicitudes anteriores.
        builder.Property(s => s.ClienteNombre).HasMaxLength(200);
        builder.Property(s => s.ProyectoNombre).HasMaxLength(200);
        builder.Property(s => s.Modalidad).HasMaxLength(100);
        builder.Property(s => s.Recurso).HasMaxLength(100);
        builder.Property(s => s.Lugar).HasMaxLength(200);
        builder.Property(s => s.Descripcion).HasMaxLength(1000);

        builder.HasOne<Proyecto>()
            .WithMany()
            .HasForeignKey(s => s.ProyectoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(s => s.TieneRegistro);

        builder.HasIndex(s => new { s.UserId, s.FechaRegistro });
    }
}
