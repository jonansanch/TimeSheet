using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class ReporteUsuarioConfiguration : IEntityTypeConfiguration<ReporteUsuario>
{
    public void Configure(EntityTypeBuilder<ReporteUsuario> builder)
    {
        builder.ToTable("ReportesUsuario");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).HasMaxLength(450).IsRequired();
        builder.Property(r => r.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Descripcion).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(r => r.ComentarioRespuesta).HasMaxLength(2000);
        builder.Property(r => r.RespondidoPorUserId).HasMaxLength(450);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.Estado);
    }
}
