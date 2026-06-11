using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleMileageReadingConfiguration : IEntityTypeConfiguration<VehicleMileageReading>
{
    public void Configure(EntityTypeBuilder<VehicleMileageReading> builder)
    {
        builder.ToTable("VehicleMileageReadings");
        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Vehicle)
               .WithMany(v => v.MileageReadings)
               .HasForeignKey(r => r.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        // El acceso típico es "las últimas lecturas de este vehículo" — index compuesto.
        builder.HasIndex(r => new { r.VehicleId, r.CreatedAt });
    }
}
