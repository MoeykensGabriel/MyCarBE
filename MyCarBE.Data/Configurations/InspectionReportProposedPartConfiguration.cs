using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class InspectionReportProposedPartConfiguration : IEntityTypeConfiguration<InspectionReportProposedPart>
{
    public void Configure(EntityTypeBuilder<InspectionReportProposedPart> builder)
    {
        builder.ToTable("InspectionReportProposedParts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProductCode).HasMaxLength(100);
        builder.Property(x => x.EstimatedUnitPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.InspectionReport)
               .WithMany(r => r.ProposedParts)
               .HasForeignKey(x => x.InspectionReportId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.InspectionReportId);

        builder.HasQueryFilter(x => !x.IsDeleted && !x.InspectionReport.IsDeleted);
    }
}
