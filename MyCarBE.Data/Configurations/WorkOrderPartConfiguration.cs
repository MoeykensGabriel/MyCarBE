using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Configurations;

public class WorkOrderPartConfiguration : IEntityTypeConfiguration<WorkOrderPart>
{
    public void Configure(EntityTypeBuilder<WorkOrderPart> builder)
    {
        builder.ToTable("WorkOrderParts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.ProductCode).HasMaxLength(100);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(p => p.CustomerUnitPrice).HasColumnType("numeric(18,2)");

        // Propiedades calculadas — no se persisten.
        builder.Ignore(p => p.Subtotal);
        builder.Ignore(p => p.EffectiveCustomerUnitPrice);
        builder.Ignore(p => p.CustomerSubtotal);

        builder.Property(p => p.Tier)
               .HasConversion<int>()
               .HasDefaultValue(WorkOrderPartTier.Generic);

        builder.Property(p => p.ApprovalStatus)
               .HasConversion<int>()
               .HasDefaultValue(QuoteItemApprovalStatus.Pending);

        builder.HasOne(p => p.WorkOrder)
               .WithMany(w => w.Parts)
               .HasForeignKey(p => p.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Índices para queries frecuentes
        builder.HasIndex(p => p.WorkOrderId);
        builder.HasIndex(p => p.AlternativeGroupId);
        builder.HasIndex(p => p.ProductCode);
    }
}
