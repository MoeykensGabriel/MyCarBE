using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkOrderServiceConfiguration : IEntityTypeConfiguration<WorkOrderService>
{
    public void Configure(EntityTypeBuilder<WorkOrderService> builder)
    {
        builder.ToTable("WorkOrderServices");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.NameSnapshot).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DescriptionSnapshot).IsRequired().HasMaxLength(1000);
        builder.Property(s => s.PriceSnapshot).HasColumnType("numeric(18,2)");

        // Snapshots: se copian al crear y no se propagan cambios del catálogo
        builder.Property(s => s.NameSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(s => s.DescriptionSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(s => s.PriceSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        builder.HasOne(s => s.WorkOrder)
               .WithMany(w => w.Services)
               .HasForeignKey(s => s.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.CatalogService)
               .WithMany()
               .HasForeignKey(s => s.CatalogServiceId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
