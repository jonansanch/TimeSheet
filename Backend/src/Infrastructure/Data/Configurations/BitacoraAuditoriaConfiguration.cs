using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class BitacoraAuditoriaConfiguration : IEntityTypeConfiguration<BitacoraAuditoria>
{
    public void Configure(EntityTypeBuilder<BitacoraAuditoria> builder)
    {
        builder.HasKey(b => b.Id);
        builder.Property(b => b.TipoEvento).IsRequired().HasMaxLength(100);
        builder.Property(b => b.ActorId).IsRequired().HasMaxLength(450);
        builder.Property(b => b.ActorEmail).HasMaxLength(256);
        builder.Property(b => b.EntidadAfectada).IsRequired().HasMaxLength(100);
        builder.Property(b => b.EntidadId).HasMaxLength(450);
        builder.Property(b => b.MetadataJson).HasMaxLength(4000);
        builder.HasIndex(b => b.Timestamp);
        builder.HasIndex(b => b.ActorId);
        builder.HasIndex(b => b.TipoEvento);
    }
}
