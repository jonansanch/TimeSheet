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
        builder.Property(p => p.Valor).IsRequired().HasMaxLength(500);
        builder.HasIndex(p => p.Clave).IsUnique();
    }
}
