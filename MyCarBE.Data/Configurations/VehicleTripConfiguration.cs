using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleTripConfiguration : IEntityTypeConfiguration<VehicleTrip>
{
    public void Configure(EntityTypeBuilder<VehicleTrip> builder)
    {
        builder.ToTable("VehicleTrips");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.DriverName).IsRequired().HasMaxLength(150);
        builder.Property(t => t.DriverDocument).IsRequired().HasMaxLength(30);
        builder.Property(t => t.Status).HasConversion<int>();

        builder.HasOne(t => t.Vehicle)
               .WithMany(v => v.Trips)
               .HasForeignKey(t => t.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.VehicleId);
        builder.HasIndex(t => new { t.VehicleId, t.Status });
        builder.HasIndex(t => t.StartedAt);
    }
}
