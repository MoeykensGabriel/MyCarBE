using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleTireMeasurementConfiguration : IEntityTypeConfiguration<VehicleTireMeasurement>
{
    public void Configure(EntityTypeBuilder<VehicleTireMeasurement> builder)
    {
        builder.ToTable("VehicleTireMeasurements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InnerDepthMm).HasPrecision(5, 2);
        builder.Property(m => m.CenterDepthMm).HasPrecision(5, 2);
        builder.Property(m => m.OuterDepthMm).HasPrecision(5, 2);
        builder.Property(m => m.Notes).HasMaxLength(1000);

        builder.HasOne(m => m.VehicleTire)
               .WithMany(t => t.Measurements)
               .HasForeignKey(m => m.VehicleTireId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.VehicleTireId);
        builder.HasIndex(m => new { m.VehicleTireId, m.MeasuredOn });

        // Average y Spread son computed → ignorados por EF
        builder.Ignore(m => m.AverageDepthMm);
        builder.Ignore(m => m.SpreadMm);
    }
}
