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

        builder.Property(r => r.HoraEntradaAM).IsRequired(false);
        builder.Property(r => r.HoraSalidaAM).IsRequired(false);
        builder.Property(r => r.HoraEntradaPM).IsRequired(false);
        builder.Property(r => r.HoraSalidaPM).IsRequired(false);

        builder.Property(r => r.Cliente).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Proyecto).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Modalidad).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Recurso).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Lugar).HasMaxLength(200).IsRequired();
        builder.Property(r => r.EsRetroactivo).IsRequired().HasDefaultValue(false);

        builder.Ignore(r => r.TieneAM);
        builder.Ignore(r => r.TienePM);

        // Un registro por usuario/día/proyecto (puede haber varios proyectos el mismo día)
        builder.HasIndex(r => new { r.UserId, r.FechaRegistro, r.Cliente, r.Proyecto })
            .HasDatabaseName("IX_RegistrosHoras_UserId_FechaRegistro_Cliente_Proyecto")
            .IsUnique();

        builder.HasIndex(r => r.FechaRegistro)
            .HasDatabaseName("IX_RegistrosHoras_FechaRegistro")
            .IncludeProperties(r => r.UserId);
    }
}
