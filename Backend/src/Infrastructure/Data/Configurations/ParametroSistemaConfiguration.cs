using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class ParametroSistemaConfiguration : IEntityTypeConfiguration<ParametroSistema>
{
    public void Configure(EntityTypeBuilder<ParametroSistema> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Clave).IsRequired().HasMaxLength(100);

        // Sin HasMaxLength ni HasColumnType: EF deja el ancho sin especificar, que en SQL
        // Server cae en nvarchar(max) y en SQLite (usado en tests) no rompe la sintaxis de
        // creacion de tabla. Hace falta: la mayoria de los valores son cortos (numeros,
        // "Semanal"), pero el logo de reportes guarda una imagen entera en base64.
        builder.Property(p => p.Valor).IsRequired();

        builder.HasIndex(p => p.Clave).IsUnique();
    }
}
