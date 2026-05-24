using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class NotificacionEnviadaConfiguration : IEntityTypeConfiguration<NotificacionEnviada>
{
    public void Configure(EntityTypeBuilder<NotificacionEnviada> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.UserId).IsRequired().HasMaxLength(450);
        builder.Property(n => n.Email).IsRequired().HasMaxLength(256);
        builder.Property(n => n.ErrorDetalle).HasMaxLength(2000);
        builder.HasIndex(n => new { n.UserId, n.Created });
        builder.HasIndex(n => n.Created)
            .HasDatabaseName("IX_NotificacionesEnviadas_Created");
    }
}
