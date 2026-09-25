using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class TerminoDescripcionConfiguration : IEntityTypeConfiguration<TerminoDescripcion>
{
    public void Configure(EntityTypeBuilder<TerminoDescripcion> builder)
    {
        builder.ToTable("TerminosDescripcion");

        builder.Property(t => t.Termino).HasMaxLength(100).IsRequired();
        builder.Property(t => t.TerminoNormalizado).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Motivo).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Sugerencia).HasMaxLength(300);

        builder.Property(t => t.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Regla).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.Severidad).HasConversion<string>().HasMaxLength(20).IsRequired();

        // Dos terminos que normalizan igual (tildes/mayusculas aparte) serian el mismo
        // termino para el evaluador: se evita duplicarlos en el catalogo.
        builder.HasIndex(t => t.TerminoNormalizado).IsUnique();

        // El evaluador solo consulta los activos en cada guardado; conviene que sea rapido.
        builder.HasIndex(t => t.Activo);
    }
}
