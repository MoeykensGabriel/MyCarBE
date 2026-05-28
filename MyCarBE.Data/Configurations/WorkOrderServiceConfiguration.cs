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
        builder.Property(s => s.EstimatedDurationMinutesSnapshot).HasDefaultValue(0);

        // Snapshots: se copian al crear y no se propagan cambios del catálogo
        builder.Property(s => s.NameSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(s => s.DescriptionSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(s => s.PriceSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
        builder.Property(s => s.EstimatedDurationMinutesSnapshot).Metadata.SetAfterSaveBehavior(
            Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);

        builder.HasOne(s => s.WorkOrder)
               .WithMany(w => w.Services)
               .HasForeignKey(s => s.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        // CatalogService es opcional: cuando el servicio es ad-hoc (puntual) queda null.
        builder.HasOne(s => s.CatalogService)
               .WithMany()
               .HasForeignKey(s => s.CatalogServiceId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        // ── Asignación al mecánico ──────────────────────────────────────────
        builder.Property(s => s.AssignmentStatus)
               .HasConversion<int>()
               .HasDefaultValue(Domain.Enums.WorkOrderServiceAssignmentStatus.Unassigned);

        builder.Property(s => s.MechanicNotes).HasMaxLength(2000);
        builder.Property(s => s.MechanicFindings).HasMaxLength(2000);

        builder.HasOne(s => s.AssignedMechanic)
               .WithMany(m => m.AssignedServices)
               .HasForeignKey(s => s.AssignedMechanicId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.AssignedMechanicId);
        builder.HasIndex(s => s.AssignmentStatus);

        // ── Área (para agrupar el calendario) ────────────────────────────────
        builder.HasOne(s => s.Area)
               .WithMany()
               .HasForeignKey(s => s.AreaId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.AreaId);
        builder.HasIndex(s => new { s.ScheduledStart, s.ScheduledEnd });

        // ── Cotización item-por-item ──────────────────────────────────────────
        builder.Property(s => s.ApprovalStatus)
               .HasConversion<int>()
               .HasDefaultValue(Domain.Enums.QuoteItemApprovalStatus.Pending);

        builder.HasIndex(s => s.AlternativeGroupId);

        // ── Concurrencia ─────────────────────────────────────────────────────
        // Optimistic concurrency token usando la columna xmin del sistema en
        // Postgres (sin nueva columna física — shadow property mapeada a xmin,
        // que PG ya mantiene por cada fila).
        // Crítico para el ClaimCommand (S3.5): dos mecánicos clickeando
        // "Tomar trabajo" al mismo tiempo — el segundo recibe
        // DbUpdateConcurrencyException y el handler lo traduce a 409 Conflict.
        builder.Property<uint>("xmin")
               .HasColumnType("xid")
               .HasColumnName("xmin")
               .ValueGeneratedOnAddOrUpdate()
               .IsConcurrencyToken();
    }
}
