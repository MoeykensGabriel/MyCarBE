using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCarBE.Domain.Entities;

namespace MyCarBE.Data.Configurations;

public class WorkOrderApprovalTokenConfiguration : IEntityTypeConfiguration<WorkOrderApprovalToken>
{
    public void Configure(EntityTypeBuilder<WorkOrderApprovalToken> builder)
    {
        builder.ToTable("WorkOrderApprovalTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
               .IsRequired()
               .HasMaxLength(128);

        builder.HasIndex(t => t.Token).IsUnique();
        builder.HasIndex(t => t.WorkOrderId);

        builder.HasOne(t => t.WorkOrder)
               .WithMany()
               .HasForeignKey(t => t.WorkOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
