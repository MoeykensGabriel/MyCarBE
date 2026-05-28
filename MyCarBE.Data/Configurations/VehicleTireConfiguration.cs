using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleTireConfiguration : IEntityTypeConfiguration<VehicleTire>
{
    public void Configure(EntityTypeBuilder<VehicleTire> builder)
    {
        builder.ToTable("VehicleTires");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Position).HasConversion<int>();
        builder.Property(t => t.Brand).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Model).IsRequired().HasMaxLength(100);
        builder.Property(t => t.SizeSpec).IsRequired().HasMaxLength(50);

        builder.Property(t => t.InitialTreadDepthMm).HasPrecision(5, 2);

        builder.HasOne(t => t.Vehicle)
               .WithMany(v => v.Tires)
               .HasForeignKey(t => t.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.VehicleId);
        // Solo una cubierta activa por (vehicle, position). EF Core soporta filtered unique
        // index via HasFilter — usamos un index único parcial en Postgres.
        builder.HasIndex(t => new { t.VehicleId, t.Position })
               .IsUnique()
               .HasFilter("\"IsActive\" = TRUE AND \"IsDeleted\" = FALSE");
    }
}
