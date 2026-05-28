using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class InspectionReportProposedServiceConfiguration : IEntityTypeConfiguration<InspectionReportProposedService>
{
    public void Configure(EntityTypeBuilder<InspectionReportProposedService> builder)
    {
        builder.ToTable("InspectionReportProposedServices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.EstimatedLaborCost).HasPrecision(18, 2);

        builder.HasOne(x => x.InspectionReport)
               .WithMany(r => r.ProposedServices)
               .HasForeignKey(x => x.InspectionReportId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.InspectionReportId);

        builder.HasQueryFilter(x => !x.IsDeleted && !x.InspectionReport.IsDeleted);
    }
}
