using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class MaintenanceAlertConfiguration : IEntityTypeConfiguration<MaintenanceAlert>
{
    public void Configure(EntityTypeBuilder<MaintenanceAlert> builder)
    {
        builder.ToTable("MaintenanceAlerts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(1000);

        builder.HasOne(a => a.Vehicle)
               .WithMany(v => v.MaintenanceAlerts)
               .HasForeignKey(a => a.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
