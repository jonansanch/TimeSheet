using KPG.Timesheet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KPG.Timesheet.Infrastructure.Data.Configurations;

public class LugarTrabajoConfiguration : IEntityTypeConfiguration<LugarTrabajo>
{
    public void Configure(EntityTypeBuilder<LugarTrabajo> builder)
    {
        builder.ToTable("LugaresTrabajo");
        builder.Property(l => l.Nombre).HasMaxLength(200).IsRequired();
        builder.HasIndex(l => l.Nombre).IsUnique();
    }
}
