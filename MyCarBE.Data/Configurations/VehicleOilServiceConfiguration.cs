using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class VehicleOilServiceConfiguration : IEntityTypeConfiguration<VehicleOilService>
{
    public void Configure(EntityTypeBuilder<VehicleOilService> builder)
    {
        builder.ToTable("VehicleOilServices");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OilType).HasMaxLength(100);
        builder.Property(o => o.OilBrand).HasMaxLength(100);
        builder.Property(o => o.Notes).HasMaxLength(2000);
        builder.Property(o => o.IntervalKm).HasDefaultValue(10000);
        builder.Property(o => o.IntervalMonths).HasDefaultValue(6);
        builder.Property(o => o.FilterChanged).HasDefaultValue(true);

        builder.HasOne(o => o.Vehicle)
               .WithMany(v => v.OilServices)
               .HasForeignKey(o => o.VehicleId)
               .OnDelete(DeleteBehavior.Cascade);

        // Trazabilidad opcional a la orden — sin cascade para no borrar el histórico de service.
        builder.HasOne(o => o.WorkOrder)
               .WithMany()
               .HasForeignKey(o => o.WorkOrderId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => o.VehicleId);
    }
}
