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

        // Default a nivel DB para que la fila existente quede en 14 al migrar
        // (sin esto, AddColumn la dejaría en 0 y el recordatorio saltaría siempre).
        builder.Property(s => s.MileageReminderDays)
            .IsRequired()
            .HasDefaultValue(14);
    }
}
