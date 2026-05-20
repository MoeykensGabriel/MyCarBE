using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(w => w.CustomerNote).HasMaxLength(1000);
        builder.Property(w => w.TechnicianNote).HasMaxLength(1000);

        // CustomerIdAtEntry, FleetIdAtEntry y CreatedByUserId se congelan al crear — nunca se modifican
        builder.Property(w => w.CustomerIdAtEntry).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(w => w.FleetIdAtEntry).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(w => w.CreatedByUserId).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        builder.HasIndex(w => w.CreatedByUserId);

        builder.HasOne(w => w.Vehicle)
               .WithMany(v => v.WorkOrders)
               .HasForeignKey(w => w.VehicleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CustomerAtEntry)
               .WithMany()
               .HasForeignKey(w => w.CustomerIdAtEntry)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasOne(w => w.FleetAtEntry)
               .WithMany()
               .HasForeignKey(w => w.FleetIdAtEntry)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);
    }
}
