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
        builder.Property(r => r.Turno).HasConversion<string>().HasMaxLength(2).IsRequired();
        builder.Property(r => r.HoraEntrada).IsRequired();
        builder.Property(r => r.HoraSalida).IsRequired();
        builder.Property(r => r.Cliente).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Proyecto).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Modalidad).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Recurso).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.Lugar).HasMaxLength(200).IsRequired();
        builder.Property(r => r.EsRetroactivo).IsRequired().HasDefaultValue(false);

        builder.HasIndex(r => r.FechaRegistro);
    }
}
