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

        builder.HasIndex(s => new { s.UserId, s.FechaRegistro });
    }
}
