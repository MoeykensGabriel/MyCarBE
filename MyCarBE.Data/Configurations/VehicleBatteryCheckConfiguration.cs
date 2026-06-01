using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleBatteryCheckConfiguration : IEntityTypeConfiguration<VehicleBatteryCheck>
{
    public void Configure(EntityTypeBuilder<VehicleBatteryCheck> builder)
    {
        builder.ToTable("VehicleBatteryChecks");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).HasConversion<int>();
        builder.Property(c => c.Voltage).HasPrecision(5, 2);
        builder.Property(c => c.Notes).HasMaxLength(1000);

        builder.HasOne(c => c.VehicleBattery)
               .WithMany(b => b.Checks)
               .HasForeignKey(c => c.VehicleBatteryId)
               .OnDelete(DeleteBehavior.Cascade);

        // Vínculo opcional con la orden. Restrict: borrar una orden NO borra el historial
        // de chequeos de la batería.
        builder.HasOne(c => c.WorkOrder)
               .WithMany()
               .HasForeignKey(c => c.WorkOrderId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.VehicleBatteryId);
        builder.HasIndex(c => new { c.VehicleBatteryId, c.CheckedOn });
        builder.HasIndex(c => c.WorkOrderId);
    }
}
