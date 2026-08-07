using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;
using MyCarBE.Domain.Enums;

namespace MyCarBE.Data.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("WorkOrders");
        builder.HasKey(w => w.Id);

        // Número visible de la orden. Lo genera Postgres (identity by default, arranca en 1000):
        // "by default" y no "always" para que el backfill de la migración pueda escribir los
        // números de las órdenes históricas. Nunca se modifica una vez asignado.
        builder.Property(w => w.Number)
               .UseIdentityByDefaultColumn()
               .HasIdentityOptions(startValue: 1000, incrementBy: 1);

        builder.Property(w => w.Number).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        builder.HasIndex(w => w.Number).IsUnique();

        builder.Property(w => w.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(w => w.PurchaseOrderNumber).HasMaxLength(100);
        builder.Property(w => w.DepositAmount).HasColumnType("numeric(18,2)");
        builder.Property(w => w.CustomerNote).HasMaxLength(1000);
        builder.Property(w => w.TechnicianNote).HasMaxLength(1000);
        builder.Property(w => w.ServiceReason).HasMaxLength(2000);

        // El propósito se declara en el ingreso y se congela: la orden ENTRÓ como solo
        // inspección y eso no se reescribe ni al promoverla (para eso está PromotedToRepairAt).
        // Es lo que permite saber después que nunca pasó por diagnóstico.
        builder.Property(w => w.Purpose).HasDefaultValue(WorkOrderPurpose.Repair);
        builder.Property(w => w.Purpose).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        // CustomerIdAtEntry, FleetIdAtEntry y CreatedByUserId se congelan al crear — nunca se modifican
        builder.Property(w => w.CustomerIdAtEntry).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(w => w.FleetIdAtEntry).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(w => w.CreatedByUserId).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        builder.HasIndex(w => w.CreatedByUserId);

        // Para el TTL cleanup (S4-07): filtra órdenes vencidas en AwaitingApproval.
        // Índice parcial para no inflar la tabla innecesariamente.
        builder.HasIndex(w => w.QuoteExpiresAt)
               .HasFilter("\"QuoteExpiresAt\" IS NOT NULL");

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
