using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Configurations;

public class PartsStockRequestConfiguration : IEntityTypeConfiguration<PartsStockRequest>
{
    public void Configure(EntityTypeBuilder<PartsStockRequest> builder)
    {
        builder.ToTable("PartsStockRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.LicensePlateSnapshot)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(r => r.ExternalReference)
               .HasMaxLength(100);

        builder.Property(r => r.OcNumberSnapshot).HasMaxLength(100);
        builder.Property(r => r.DepositAmountSnapshot).HasColumnType("numeric(18,2)");

        builder.Property(r => r.Status)
               .HasConversion<int>()
               .HasDefaultValue(StockRequestStatus.PendingReview);

        builder.HasOne(r => r.WorkOrder)
               .WithMany() // WorkOrder no necesita conocer sus stock requests para no inflar el modelo
               .HasForeignKey(r => r.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // Una WO puede tener varios pedidos: el original de la aprobación del presupuesto
        // + adicionales por items aprobados durante la reparación (DecideAdditionalItems).
        // La idempotencia la garantiza el orchestrator delta-based (no repite repuestos ya pedidos).
        builder.HasIndex(r => r.WorkOrderId);
        builder.HasIndex(r => r.LicensePlateSnapshot);
        builder.HasIndex(r => r.Status);
    }
}
