using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class ParametroRestriccionDiaConfiguration : IEntityTypeConfiguration<ParametroRestriccionDia>
{
    public void Configure(EntityTypeBuilder<ParametroRestriccionDia> builder)
    {
        builder.ToTable("ParametrosRestriccionDia");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.DiaDelaSemana).IsRequired().HasConversion<int>();
        builder.Property(r => r.UserId).HasMaxLength(450);
        builder.Property(r => r.Rol).HasMaxLength(100);
        builder.Property(r => r.Activo).IsRequired().HasDefaultValue(true);

        // Una sola regla por dia y persona, y una sola por dia y rol. Filtrados porque la
        // columna que no aplica queda NULL, y un unique normal trataria esos NULL como iguales.
        builder.HasIndex(r => new { r.DiaDelaSemana, r.UserId })
            .HasDatabaseName("IX_ParametrosRestriccionDia_Dia_UserId")
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex(r => new { r.DiaDelaSemana, r.Rol })
            .HasDatabaseName("IX_ParametrosRestriccionDia_Dia_Rol")
            .IsUnique()
            .HasFilter("[Rol] IS NOT NULL");

        builder.Ignore(r => r.EsDeUsuario);
    }
}
