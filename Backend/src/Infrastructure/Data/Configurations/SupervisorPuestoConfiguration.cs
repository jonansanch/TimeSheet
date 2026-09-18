using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class SupervisorPuestoConfiguration : IEntityTypeConfiguration<SupervisorPuesto>
{
    public void Configure(EntityTypeBuilder<SupervisorPuesto> builder)
    {
        builder.ToTable("SupervisoresPuesto");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SupervisorUserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.Activo).IsRequired().HasDefaultValue(true);

        builder.HasOne<Empleado>()
            .WithMany()
            .HasForeignKey(s => s.PuestoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(s => s.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una sola regla por (puesto, cliente). El indice unico filtrado deja convivir
        // la regla general (ClienteId NULL) con las especificas del mismo puesto, porque
        // en SQL Server varios NULL cuentan como iguales en un indice unico normal.
        builder.HasIndex(s => new { s.PuestoId, s.ClienteId })
            .HasDatabaseName("IX_SupervisoresPuesto_PuestoId_ClienteId")
            .IsUnique()
            .HasFilter("[ClienteId] IS NOT NULL");

        builder.HasIndex(s => s.PuestoId)
            .HasDatabaseName("IX_SupervisoresPuesto_PuestoId_General")
            .IsUnique()
            .HasFilter("[ClienteId] IS NULL");

        builder.Ignore(s => s.EsEspecificaDeCliente);
    }
}
