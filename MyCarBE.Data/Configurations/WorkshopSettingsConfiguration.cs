using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkshopSettingsConfiguration : IEntityTypeConfiguration<WorkshopSettings>
{
    public void Configure(EntityTypeBuilder<WorkshopSettings> builder)
    {
        builder.ToTable("WorkshopSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.PhysicalCapacity)
            .IsRequired()
            .HasDefaultValue(6);
    }
}
