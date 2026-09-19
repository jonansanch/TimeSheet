using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class AprobacionRegistroConfiguration : IEntityTypeConfiguration<AprobacionRegistro>
{
    public void Configure(EntityTypeBuilder<AprobacionRegistro> builder)
    {
        builder.ToTable("AprobacionesRegistro");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Accion).HasMaxLength(50).IsRequired();
        builder.Property(a => a.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Comentario).HasMaxLength(1000);

        // Cascade: el historial de aprobacion no tiene sentido sin su registro.
        builder.HasOne<RegistroHoras>()
            .WithMany()
            .HasForeignKey(a => a.RegistroHorasId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.RegistroHorasId)
            .HasDatabaseName("IX_AprobacionesRegistro_RegistroHorasId");
    }
}
