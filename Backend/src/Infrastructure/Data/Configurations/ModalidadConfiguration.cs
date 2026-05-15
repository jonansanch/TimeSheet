using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class ModalidadConfiguration : IEntityTypeConfiguration<Modalidad>
{
    public void Configure(EntityTypeBuilder<Modalidad> builder)
    {
        builder.ToTable("Modalidades");
        builder.Property(m => m.Nombre).HasMaxLength(100).IsRequired();
        builder.HasIndex(m => m.Nombre).IsUnique();
    }
}
