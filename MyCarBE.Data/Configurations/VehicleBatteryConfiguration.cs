using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleBatteryConfiguration : IEntityTypeConfiguration<VehicleBattery>
{
    public void Configure(EntityTypeBuilder<VehicleBattery> builder)
    {
        builder.ToTable("VehicleBatteries");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Brand).HasMaxLength(100);

        builder.HasOne(b => b.Vehicle)
               .WithMany(v => v.Batteries)
               .HasForeignKey(b => b.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.VehicleId);
        // Solo una batería activa por vehículo (índice único parcial en Postgres).
        builder.HasIndex(b => b.VehicleId)
               .IsUnique()
               .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE")
               .HasDatabaseName("IX_VehicleBatteries_VehicleId_Active");
    }
}
