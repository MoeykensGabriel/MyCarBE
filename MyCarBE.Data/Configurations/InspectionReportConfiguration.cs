using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class InspectionReportConfiguration : IEntityTypeConfiguration<InspectionReport>
{
    public void Configure(EntityTypeBuilder<InspectionReport> builder)
    {
        builder.ToTable("InspectionReports");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Findings).HasMaxLength(4000);
        builder.Property(r => r.HasIssue).HasDefaultValue(false);
        builder.Property(r => r.IsNoFindings).HasDefaultValue(false);
        builder.Property(r => r.IsSkipped).HasDefaultValue(false);
        builder.Property(r => r.SkipReason).HasMaxLength(500);

        builder.HasOne(r => r.WorkOrder)
               .WithMany(w => w.InspectionReports)
               .HasForeignKey(r => r.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Area)
               .WithMany()
               .HasForeignKey(r => r.AreaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Mechanic)
               .WithMany()
               .HasForeignKey(r => r.MechanicId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        // Unique: una orden solo puede tener UN reporte VIVO por área. El filtro por
        // IsDeleted lo hace parcial (como en batería/cubiertas): al deshacer una marca de
        // oficina (soft-delete) el área puede volver a marcarse sin chocar con el índice.
        builder.HasIndex(r => new { r.WorkOrderId, r.AreaId })
               .IsUnique()
               .HasFilter("\"IsDeleted\" = FALSE");
        builder.HasIndex(r => r.MechanicId);

        // Query filter: el reporte hereda el soft-delete de la WorkOrder
        builder.HasQueryFilter(r => !r.IsDeleted && !r.WorkOrder.IsDeleted);
    }
}
