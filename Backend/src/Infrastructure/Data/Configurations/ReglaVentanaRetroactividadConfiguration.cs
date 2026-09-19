using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class ReglaVentanaRetroactividadConfiguration : IEntityTypeConfiguration<ReglaVentanaRetroactividad>
{
    public void Configure(EntityTypeBuilder<ReglaVentanaRetroactividad> builder)
    {
        builder.ToTable("ReglasVentanaRetroactividad");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).HasMaxLength(450);
        builder.Property(r => r.Rol).HasMaxLength(100);
        builder.Property(r => r.Dias).IsRequired();
        builder.Property(r => r.Activo).IsRequired().HasDefaultValue(true);

        // Una sola regla por persona y una sola por rol. Filtrados porque la columna que
        // no aplica queda NULL, y un unique normal trataria esos NULL como iguales.
        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_ReglasVentanaRetroactividad_UserId")
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        builder.HasIndex(r => r.Rol)
            .HasDatabaseName("IX_ReglasVentanaRetroactividad_Rol")
            .IsUnique()
            .HasFilter("[Rol] IS NOT NULL");

        builder.Ignore(r => r.EsDeUsuario);
    }
}
